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
    public class ProductLakeController : ControllerBase
    {
        private readonly AppDbContext _context;
        
        public ProductLakeController(AppDbContext context)
        {
            _context = context;
        }


        /*
         * 
         * 
         * MAYBE NOT NEEDED CHECK FRONTEND CODE FIRST THOUGH. DATA LAKE DONE ON AZURE NOW
         * 
         * 
         * */




        [HttpGet]
        [Route("GetAllProductsFromLake")]
        //[Authorize(Roles = "Product Manager")]
        public async Task<ActionResult<IEnumerable<ProductLake>>> GetAllProductsFromLake() // This needs changing
        {
            var products = await _context.Lake.Where(x => x.IsDeleted == false).ToListAsync();

            return products;
        }

        [HttpGet]
        [Route("GetDeletedProducts")]
        //[Authorize(Roles = "Product Manager")]
        public async Task<ActionResult<IEnumerable<ProductLake>>> GetDeletedProducts() // This needs changing
        {
            var products = await _context.Lake.Where(x => x.IsDeleted == true).ToListAsync();

            return products;
        }

        [HttpGet]
        [Route("GetProductByIdFromLake/{id}")]
        //[Authorize(Roles = "Product Manager")]
        public async Task<ActionResult<ProductLake>> GetProductByIdFromLake(int id) // This might have to change
        {
            var product = await _context.Lake.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        [HttpPut]
        [Route("ApproveProductUpdate/{id}")]
        //[Authorize(Roles = "Product Manager")]
        public async Task<ActionResult<ProductLake>> ApproveProductUpdate(int id) // This needs changing or MERGING with approve product in ProductsController
        {
            var update = await _context.Lake.FindAsync(id);
            if (update == null)
            {
                return NotFound();
            }

            var product = new Product()
            {
                ProductId = 0,
                ProductName = update.ProductName,
                ProductDescription = update.ProductDescription,
                ProductCategoryId = update.ProductCategoryId,
                IsApproved = true,
                IsDeleted = false
            };

            var updatedVendorProducts = _context.VendorProducts.Where(x => x.ProductId == product.ProductId).ToList();

            foreach(var updatedVendorProduct in updatedVendorProducts)
            {
                var updatedVP = new VendorProduct()
                {
                    Product = product,
                    isActive = true
                };
                _context.Update(updatedVP);
                await _context.SaveChangesAsync();
            }

            _context.Products.Add(product);
            var addResult = await _context.SaveChangesAsync();
            if (addResult > 0)
            {
                _context.Lake.Remove(update);
                var saveResult = await _context.SaveChangesAsync();
                if (saveResult > 0)
                {
                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(ex.Message);
                    }
                }
            }

            return Ok();
        }
    }
}
