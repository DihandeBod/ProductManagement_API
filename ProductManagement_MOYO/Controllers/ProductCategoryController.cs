using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductManagement_MOYO.Data;
using ProductManagement_MOYO.Models;

namespace ProductManagement_MOYO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductCategoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductCategoryController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("GetAllProductCategories")]
        public async Task<ActionResult<IEnumerable<ProductCategory>>> GetAllProductTypes()
        {
            var categories = await _context.ProductCategories.ToListAsync();
            if (categories == null || categories.Count == 0)
            {
                return NotFound();
            }

            return categories;
        }

        [HttpGet]
        [Route("GetProductCategoryById/{id}")]
        public async Task<ActionResult<ProductCategory>> GetProductTypeById(int id)
        {
            var categories = await _context.ProductCategories.FindAsync(id);
            if (categories == null)
            {
                return NotFound();
            }

            return categories;
        }
    }
}
