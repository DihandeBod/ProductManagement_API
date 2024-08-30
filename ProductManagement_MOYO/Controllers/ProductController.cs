
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductManagement_MOYO.Data;
using ProductManagement_MOYO.Models;
using ProductManagement_MOYO.ViewModels;

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
        //[Authorize(Roles = "Product Manager,Product Capturer, Customer")]
        public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts()
        {
            var products = await _context.Products.Where(x => x.IsDeleted == false).ToListAsync();
            if(products == null || products.Count == 0)
            {
                return NotFound();
            }

            return products;
        }

        [HttpGet]
        [Route("GetProductById/{id}")]
        //[Authorize(Roles = "Product Manager,Product Capturer, Customer")]
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
        //[Authorize(Roles = "Product Capturer")]
        public async Task<ActionResult<Product>> AddProduct(ProductVM vm)
        {
            var product = new Product()
            {
                ProductDescription = vm.ProductDescription,
                ProductName = vm.ProductName,
                IsDeleted = false,
                ProductCategoryId = vm.ProductCategoryId,
                IsApproved = true
            };

            _context.Add(product);
            await _context.SaveChangesAsync();

            var vendors = _context.Vendors.ToList();

            foreach (var v in vendors)
            {
                var vendorProduct = new VendorProduct()
                {
                    VendorId = v.VendorId,
                    Vendor = v,
                    ProductId = product.ProductId,
                    Product = product,
                    Price = 0.00,
                    QuantityOnHand = 0,
                    StockLimit = 5,
                    isActive = false
                };
                _context.Add(vendorProduct);
            }

            try
            {
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
        //[Authorize(Roles = "Product Capturer")]
        public async Task<ActionResult<ProductLake>> UpdateProduct(int id, ProductVM vm)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            
            var prodLake = new ProductLake()
            {
                IsApproved = false,
                IsDeleted = product.IsDeleted,
                ProductName = vm.ProductName,
                ProductDescription = vm.ProductDescription,
                ProductCategoryId = vm.ProductCategoryId,
                ProductId = 0
            };
            await _context.Lake.AddAsync(prodLake);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                product.IsDeleted = true;
                product.IsApproved = false;
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        [HttpDelete]
        [Route("DeleteProduct/{id}")]
        //[Authorize(Roles = "Product Manager")]
        public async Task<ActionResult<ProductLake>> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var prodLake = new ProductLake()
            {
                IsApproved = false,
                IsDeleted = true,
                ProductName = product.ProductName,
                ProductDescription = product.ProductDescription,
                ProductCategoryId = product.ProductCategoryId,
                ProductId = 0
            };

            await _context.Lake.AddAsync(prodLake);
            var addResult = await _context.SaveChangesAsync();
            if (addResult > 0)
            {
                _context.Products.Remove(product);
                var deleteResult = await _context.SaveChangesAsync();
                if (deleteResult > 0)
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
