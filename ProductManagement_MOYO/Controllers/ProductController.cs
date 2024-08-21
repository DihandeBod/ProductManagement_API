using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using ProductManagement_MOYO.Data;
using ProductManagement_MOYO.Models;
using ProductManagement_MOYO.ViewModels;
using System.Runtime.CompilerServices;

namespace ProductManagement_MOYO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("GetAllProducts")]
        public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts()
        {
            var products = await _context.Products.ToListAsync();
            if(products == null || products.Count == 0)
            {
                return NotFound();
            }

            return products;
        }

        [HttpGet]
        [Route("GetProductById/{id}")]
        public async Task<ActionResult<Product>> GetProductById(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        [HttpPost]
        [Route("AddProduct")]
        public async Task<ActionResult<Product>> AddProduct(ProductVM vm)
        {
            var product = new Product()
            {
                ProductDescription = vm.ProductDescription,
                ProductName = vm.ProductName,
                IsDeleted = false,
                ProductCategoryId = vm.ProductCategoryId
            };

            try
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(product);
        } 


        [HttpPut]
        [Route("UpdateProduct/{id}")]
        public async Task<ActionResult<Product>> UpdateProduct(int id, ProductVM vm)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            product.ProductName = vm.ProductName;
            product.ProductDescription = vm.ProductDescription;
            product.ProductCategoryId = vm.ProductCategoryId;

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

        [HttpDelete]
        [Route("DeleteProduct/{id}")]
        public async Task<ActionResult<Product>> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            product.IsDeleted = true;

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
    }
}
