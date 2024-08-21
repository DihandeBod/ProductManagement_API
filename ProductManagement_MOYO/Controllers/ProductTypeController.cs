using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductManagement_MOYO.Data;
using ProductManagement_MOYO.Models;
using ProductManagement_MOYO.ViewModels;

namespace ProductManagement_MOYO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductTypeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductTypeController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("GetAllProductTypes")]
        public async Task<ActionResult<IEnumerable<ProductType>>> GetAllProductTypes()
        {
            var types = await _context.ProductTypes.ToListAsync();
            if (types == null || types.Count == 0)
            {
                return NotFound();
            }

            return types;
        }

        [HttpGet]
        [Route("GetProductTypeById/{id}")]
        public async Task<ActionResult<ProductType>> GetProductTypeById(int id)
        {
            var type = await _context.ProductTypes.FindAsync(id);
            if (type == null)
            {
                return NotFound();
            }

            return type;
        }
    }
}
