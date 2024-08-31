using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductManagement_MOYO.Data;
using ProductManagement_MOYO.Models;
using ProductManagement_MOYO.ViewModels;

namespace ProductManagement_MOYO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrderController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("PlaceOrder")]
        //[Authorize("Customer")]
        public async Task<ActionResult<Order>> PlaceOrder(OrderDto orderDto)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var order = new Order
                    {
                        UserId = orderDto.UserId,
                        OrderDate = DateTime.Now,
                        OrderStatusId = 1,
                        OrderTotal = 0,
                        isAssigned = false
                    };

                    _context.Orders.Add(order);
                    var orderResult = await _context.SaveChangesAsync();
                    if (orderResult > 0)
                    {
                        foreach (var line in orderDto.orderLines)
                        {
                            var orderLine = new OrderLine
                            {
                                OrderId = order.OrderId,
                                ProductId = line.ProductId,
                                Quantity = line.Quantity
                            };
                            _context.OrderLines.Add(orderLine);
                        }
                    }
                    
                    await _context.SaveChangesAsync();

                    int orderId = order.OrderId;
                    var allocationResult = await AllocateOrder(orderId, orderDto);

                    if (allocationResult is BadRequestObjectResult || allocationResult is NotFoundObjectResult)
                    {
                        await transaction.RollbackAsync();
                        return allocationResult;
                    }

                    await transaction.CommitAsync();
                    return Ok(order);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    transaction.Dispose();
                    return BadRequest(ex.Message);
                }
            }
        }

        [HttpPut]
        [Route("AllocateOrder")]
        public async Task<ActionResult> AllocateOrder(int orderId, OrderDto orderDto)
        {
            var orderLines = orderDto.orderLines;
            var vendorDict = new Dictionary<int, double>(); // VendorId, Total Order Cost
            var vendorProductDict = new Dictionary<int, List<VendorProduct>>(); // VendorId, List of VendorProducts
            var orderToAllocate = await _context.Orders.Where(x => x.OrderId == orderId).FirstOrDefaultAsync();

            if (orderToAllocate == null)
            {
                return NotFound("Order not found");
            }

            var vendors = await _context.Vendors.ToListAsync(); // Materialize vendors list
            foreach (var vendor in vendors)
            {
                double totalCost = 0;
                bool canFulfill = true;
                var vendorProducts = new List<VendorProduct>();

                foreach (var orderLine in orderLines)
                {
                    var vendorProduct = await _context.VendorProducts
                        .Where(x => x.VendorId == vendor.VendorId && x.ProductId == orderLine.ProductId && x.isActive == true)
                        .FirstOrDefaultAsync(); // Materialize each vendorProduct

                    if (vendorProduct == null || vendorProduct.QuantityOnHand < orderLine.Quantity)
                    {
                        canFulfill = false;
                        break;
                    }

                    totalCost += vendorProduct.Price * orderLine.Quantity;
                    vendorProducts.Add(vendorProduct);
                }

                if (canFulfill)
                {
                    vendorDict[vendor.VendorId] = totalCost;
                    vendorProductDict[vendor.VendorId] = vendorProducts;
                }
            }

            if (vendorDict.Count == 0)
            {
                return BadRequest("No vendors available with sufficient inventory to fulfill the entire order.");
            }

            // Get the vendor with the lowest total order cost
            var bestVendorId = vendorDict.OrderBy(x => x.Value).First().Key;
            var bestVendorProducts = vendorProductDict[bestVendorId];

            // Allocate order
            orderToAllocate.VendorId = bestVendorId;
            orderToAllocate.OrderTotal = vendorDict[bestVendorId];
            orderToAllocate.isAssigned = true;
            orderToAllocate.OrderStatusId = 2;

            // Subtract order quantity from vendor's inventory
            foreach (var orderLine in orderLines)
            {
                var vendorProduct = bestVendorProducts.FirstOrDefault(x => x.ProductId == orderLine.ProductId);
                if (vendorProduct != null)
                {
                    vendorProduct.QuantityOnHand -= orderLine.Quantity;
                }
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet]
        [Route("ViewCustomerOrdersById/{id}")]
        public async Task<ActionResult<IEnumerable<Order>>> ViewOrdersById(int id)
        {
            var orders = await _context.Orders.Where(x => x.UserId == id).ToListAsync();

            if (orders == null || orders.Count == 0)
            {
                return NotFound();
            }

            return Ok(orders);
        }

        [HttpGet]
        [Route("GetOrderDetails/{id}")]
        public async Task<ActionResult<IEnumerable<OrderLine>>> getOrderDetails(int id)
        {
            var orderDetails = await _context.OrderLines.Where(x => x.OrderId == id).ToListAsync();
            if (orderDetails.Count == 0 || orderDetails == null)
            {
                return NotFound();
            }

            return Ok(orderDetails);
        }


    }
}
