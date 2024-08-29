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
                    return BadRequest(ex.Message);
                }
            }
        }

        [HttpPut]
        [Route("AllocateOrder")]
        public async Task<ActionResult> AllocateOrder(int orderId, OrderDto orderDto)
        {
            var orderLines = orderDto.orderLines;
            var vendorDict = new Dictionary<int, double>();
            var orderLineDict = new Dictionary<int, OrderLine>();
            var orderToAllocate = _context.Orders.Where(x => x.OrderId == orderId).FirstOrDefault();

            if(orderToAllocate == null)
            {
                return NotFound("Order not found");
            }

            for (int i = 0; i < orderLines.Count; i++) {
                var vendorPrices = _context.VendorProducts.Where(x => x.ProductId == orderLines[i].ProductId && x.isActive == true).OrderBy(x => x.Price).ToArray();

                for (int j = 0; j < vendorPrices.Length; j++)
                {
                    double orderSum;
                    if (vendorPrices[j].QuantityOnHand >= orderLines[i].Quantity)
                    {
                        if (vendorDict.TryGetValue(vendorPrices[j].VendorId, out orderSum))
                        {
                            orderSum += vendorPrices[j].Price * orderLines[i].Quantity;
                            vendorDict[vendorPrices[j].VendorId] = orderSum;
                        }
                        else
                        {
                            orderSum = vendorPrices[j].Price * orderLines[i].Quantity;
                            vendorDict.Add(vendorPrices[j].VendorId, orderSum);
                        }

                        if (!orderLineDict.ContainsKey(vendorPrices[j].VendorId))
                        {
                            orderLineDict.Add(vendorPrices[j].VendorId, orderLines[i]);
                        }
                    }
                }
            }

            if(vendorDict.Count == 0)
            {
                return BadRequest("No vendors available with sufficient inventory");
            }
            // Get the value of the lowest order cost
            var orderedDict = vendorDict.OrderBy(x => x.Value).ToArray();

            // Allocate order
            orderToAllocate.VendorId = orderedDict[0].Key;
            orderToAllocate.OrderTotal = orderedDict[0].Value;
            orderToAllocate.isAssigned = true;
            orderToAllocate.OrderStatusId = 2;

            // Subtract order quantity
            var selectedVendorProduct = _context.VendorProducts.Where(x => x.VendorId == orderedDict[0].Key && x.ProductId == orderLineDict[orderedDict[0].Key].ProductId).FirstOrDefault();
            if (selectedVendorProduct != null)
            {
                selectedVendorProduct.QuantityOnHand -= orderLineDict[orderedDict[0].Key].Quantity;
            }

            await _context.SaveChangesAsync();

            return Ok();

        }
        
    }
}
