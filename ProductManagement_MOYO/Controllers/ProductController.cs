using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProductManagement_MOYO.Data;
using ProductManagement_MOYO.Models;
using ProductManagement_MOYO.Services;
using ProductManagement_MOYO.ViewModels;
using System.IO;
using System.Text;

namespace ProductManagement_MOYO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly DataLakeService _dataLakeService;
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context, DataLakeService dataLakeService)
        {
            _context = context;
            _dataLakeService = dataLakeService;
        }

        [HttpGet]
        [Route("GetAllProducts")]
        //[Authorize(Roles = "Product Manager,Product Capturer, Customer")]
        public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts()
        {
            var products = await _context.Products.Where(x => x.IsDeleted == false && x.IsApproved == true).ToListAsync();
            if (products == null || products.Count == 0)
            {
                return NotFound();
            }

            return Ok(products);
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

            return Ok(product);
        }

        [HttpPost]
        [Route("AddProduct")]
        //[Authorize(Roles = "Product Capturer")]
        public async Task<ActionResult<Product>> AddProduct(ProductVM vm)
        {
            var product = new Product
            {
                ProductDescription = vm.ProductDescription,
                ProductName = vm.ProductName,
                IsDeleted = false,
                ProductCategoryId = vm.ProductCategoryId,
                IsApproved = false // Initially not approved
            };

            _context.Add(product);
            await _context.SaveChangesAsync();

            // Add product to the data lake under 'allProducts'
            var productJson = JsonConvert.SerializeObject(product);
            var productStream = new MemoryStream(Encoding.UTF8.GetBytes(productJson));
            await _dataLakeService.UploadFileAsync($"products/allProducts/{product.ProductId}.json", productStream);

            // Add VendorProduct entries
            var vendors = _context.Vendors.ToList();
            foreach (var v in vendors)
            {
                var vendorProduct = new VendorProduct
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
        public async Task<ActionResult> UpdateProduct(int id, ProductVM vm)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            product.ProductName = vm.ProductName;
            product.ProductDescription = vm.ProductDescription;
            product.ProductCategoryId = vm.ProductCategoryId;
            product.IsApproved = false;
            product.IsDeleted = false;

            // Convert existing product to JSON and upload it to the Data Lake under 'updates'
            var productJson = JsonConvert.SerializeObject(product);
            var productStream = new MemoryStream(Encoding.UTF8.GetBytes(productJson));
            await _dataLakeService.UploadFileAsync($"products/updates/{product.ProductId}.json", productStream);

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        [Route("ApproveProduct/{id}")]
        //[Authorize(Roles = "Product Manager")]
        public async Task<ActionResult> ApproveProduct(int id)
        {
            bool isDeletedFile = false;

            // Retrieve product JSON from the Data Lake
            var stream = await _dataLakeService.DownloadFileAsync($"products/updates/{id}.json");
            if (stream == null)
            {
                stream = await _dataLakeService.DownloadFileAsync($"products/deleted/{id}.json");
                isDeletedFile = true; // Set the flag to indicate that the product was in the deleted folder
            }

            if (stream == null)
            {
                return NotFound("Product not found in the data lake.");
            }

            using (var reader = new StreamReader(stream))
            {
                var productJson = await reader.ReadToEndAsync();
                var product = JsonConvert.DeserializeObject<Product>(productJson);

                if (product == null)
                {
                    return BadRequest("Failed to deserialize product.");
                }

                var productToUpdate = await _context.Products.FirstOrDefaultAsync(x => x.ProductId == product.ProductId);
                if (productToUpdate == null)
                {
                    return NotFound();
                }

                // Update the product in SQL Server based on the flag
                if (isDeletedFile)
                {
                    productToUpdate.IsDeleted = true;
                    productToUpdate.IsApproved = false;

                }
                else
                {
                    productToUpdate.ProductName = product.ProductName;
                    productToUpdate.ProductDescription = product.ProductDescription;
                    productToUpdate.ProductCategoryId = product.ProductCategoryId;
                    productToUpdate.IsApproved = true;
                    productToUpdate.IsDeleted = false;
                }

                await _context.SaveChangesAsync();

                // Convert the updated product back to JSON
                var updatedProductJson = JsonConvert.SerializeObject(productToUpdate);
                var updatedProductStream = new MemoryStream(Encoding.UTF8.GetBytes(updatedProductJson));

                // Update the product file in the 'allProducts' directory
                await _dataLakeService.UploadFileAsync($"products/allProducts/{product.ProductId}.json", updatedProductStream);

                if (!isDeletedFile)
                {
                    await _dataLakeService.DeleteFileAsync($"products/updates/{id}.json");
                }
                else
                {
                    var vendorProductsToUpdate = await _context.VendorProducts.Where(x => x.ProductId == id).ToListAsync();
                    if (vendorProductsToUpdate == null)
                    {
                        return NotFound();
                    }

                    foreach (var vp in vendorProductsToUpdate)
                    {
                        vp.isActive = false;
                    }

                    await _context.SaveChangesAsync();
                }

                return Ok(productToUpdate);
            }
        }



        [HttpDelete]
        [Route("DeleteProduct/{id}")]
        //[Authorize(Roles = "Product Manager")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            product.IsDeleted = true;

            // Convert product to JSON and upload it to the Data Lake under 'deleted'
            var productJson = JsonConvert.SerializeObject(product);
            var productStream = new MemoryStream(Encoding.UTF8.GetBytes(productJson));
            await _dataLakeService.UploadFileAsync($"products/deleted/{product.ProductId}.json", productStream);

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        [Route("SyncAllProductsToDataLake")]
        //[Authorize(Roles = "Product Manager")]
        public async Task<ActionResult> SyncAllProductsToDataLake()
        {
            int count = 0;
            var products = await _context.Products.ToListAsync();
            if (products == null || !products.Any())
            {
                return NotFound("No products found to sync.");
            }

            foreach (var product in products)
            {
                // Check if the file already exists in the Data Lake
                var fileExists = await _dataLakeService.FileExistsAsync($"products/allProducts/{product.ProductId}.json");
                if (!fileExists)
                {
                    var jsonContent = JsonConvert.SerializeObject(product);
                    var productStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent));

                    await _dataLakeService.UploadFileAsync($"products/allProducts/{product.ProductId}.json", productStream);
                    count++;
                }
            }

            return Ok(count + " Products synchronized to the Data Lake. Existing files were skipped.");
        }

    }
}
