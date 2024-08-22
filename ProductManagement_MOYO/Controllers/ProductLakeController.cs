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

        [HttpGet]
        [Route("GetAllProductsFromLake")]
        public async Task<ActionResult<IEnumerable<ProductLake>>> GetAllProductsFromLake()
        {
            var products = await _context.Lake.ToListAsync();
            if (products == null || products.Count == 0)
            {
                return NotFound();
            }

            return products;
        }

        [HttpGet]
        [Route("GetProductByIdFromLake/{id}")]
        public async Task<ActionResult<ProductLake>> GetProductByIdFromLake(int id)
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
        public async Task<ActionResult<ProductLake>> ApproveProductUpdate(int id)
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
                IsDeleted = update.IsDeleted
            };

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
