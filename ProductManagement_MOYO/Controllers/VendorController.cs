using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductManagement_MOYO.Data;
using ProductManagement_MOYO.Models;
using ProductManagement_MOYO.ViewModels;
using System.Data;

namespace ProductManagement_MOYO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendorController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VendorController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("GetAllVendors")]
        public async Task<ActionResult<IEnumerable<Vendor>>> GetAllVendors()
        {
            var vendors = await _context.Vendors.ToListAsync();
            if (vendors == null || vendors.Count == 0)
            {
                return NotFound();
            }
            return vendors;
        }

        [HttpGet]
        [Route("VendorProducts/{id}")]
        public async Task<ActionResult<IEnumerable<VendorProduct>>> GetallVendorProducts(int id)
        {
            var products = await _context.VendorProducts.Where(x => x.VendorId == id).ToListAsync();
            if (products == null || products.Count == 0)
            {
                return NotFound();
            }

            return products;
        }

        [HttpPut]
        [Route("ConfigureProduct")]
        //[Authorize("Vendor")]
        public async Task<ActionResult<Product>> ConfigureProduct(VendorConfigureDto configure)
        {
            var vendorProduct = _context.VendorProducts.Where(x => x.ProductId == configure.ProductId && x.VendorId == configure.VendorId).FirstOrDefault();
            if (vendorProduct == null)
            {
                return NotFound();
            }

            vendorProduct.Price = configure.Price;
            vendorProduct.QuantityOnHand = configure.stockCount;
            vendorProduct.StockLimit = configure.stockLimit;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        [HttpGet]
        [Route("ViewAllAllocatedOrders/{id}")]
        public async Task<ActionResult<Order>> ViewAllAllocatedOrders(int id)
        {
            var orders = _context.Orders.Where(x => x.VendorId == id && x.isAssigned == true);
            if (orders == null)
            {
                return NotFound();
            }

            return Ok(orders);
        }

        [HttpPut]
        [Route("UpdateOrderStatus/{id}")]
        public async Task<ActionResult<Order>> UpdateOrderStatus(int id)
        {
            var order = _context.Orders.Where(x => x.OrderId == id).FirstOrDefault();
            if (order == null)
            {
                return BadRequest();
            }

            switch (order.OrderStatusId)
            {
                case 1:
                    order.OrderStatusId = 2; break;

                case 2:
                    order.OrderStatusId = 3; break;

                case 3:
                    order.OrderStatusId = 4; break;

                case 4:
                    order.OrderStatusId = 5; break;

                default:
                    order.OrderStatusId = order.OrderStatusId; break;
            }

            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                await _context.SaveChangesAsync();
            }

            return Ok(order);
        }

        [HttpGet]
        [Route("GetStatusById/{id}")]
        public async Task<ActionResult<OrderStatus>> GetStatusById(int id)
        {
            var status = _context.OrderStatuses.Where(x => x.OrderStatusId == id).FirstOrDefault();
            if(status == null)
            {
                return NotFound();
            }
            return status;
        }
    }
}
