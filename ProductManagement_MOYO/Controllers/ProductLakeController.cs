using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProductManagement_MOYO.Data;
using ProductManagement_MOYO.Models;
using ProductManagement_MOYO.Services;
using ProductManagement_MOYO.ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ProductManagement_MOYO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductLakeController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly DataLakeService _dataLakeService;
        
        public ProductLakeController(AppDbContext context, DataLakeService dataLakeService)
        {
            _context = context;
            _dataLakeService = dataLakeService;
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
        public async Task<ActionResult<IEnumerable<ProductLake>>> GetAllProductsFromLake()
        {
            var products = new List<ProductLake>();
            var files = await _dataLakeService.GetFilesAsync($"products/updates/");

            foreach (var file in files)
            {
                var fileContent = await _dataLakeService.ReadFileAsync(file);

                // Deserialize the single product object from the file
                var product = JsonConvert.DeserializeObject<ProductLake>(fileContent);

                if (product != null && !product.IsDeleted)
                {
                    products.Add(product);
                }
            }

            return products;
        }


        [HttpGet]
        [Route("GetDeletedProducts")]
        //[Authorize(Roles = "Product Manager")]
        public async Task<ActionResult<IEnumerable<ProductLake>>> GetDeletedProducts()
        {
            var products = new List<ProductLake>();
            var files = await _dataLakeService.GetFilesAsync($"products/deleted/");

            foreach (var file in files)
            {
                var fileContent = await _dataLakeService.ReadFileAsync(file);

                // Deserialize the single product object from the file
                var product = JsonConvert.DeserializeObject<ProductLake>(fileContent);

                if (product != null && product.IsDeleted)
                {
                    products.Add(product);
                }
            }

            return products;
        }


        [HttpGet]
        [Route("GetProductByIdFromLake/{id}")]
        //[Authorize(Roles = "Product Manager")]
        public async Task<ActionResult<ProductLake>> GetProductByIdFromLake(int id)
        {
            string fileName = $"{id}.json"; 

            string updatedFolder = "updates";
            var updatedFileContent = await _dataLakeService.ReadFileAsync($"products/{updatedFolder}/{fileName}");

            ProductLake product = null;

            if (updatedFileContent != null)
            {
                product = JsonConvert.DeserializeObject<ProductLake>(updatedFileContent);
                return product;
            }

            string deletedFolder = "deleted";
            var deletedFileContent = await _dataLakeService.ReadFileAsync($"products/{deletedFolder}/{fileName}");

            if (deletedFileContent != null)
            {
                product = JsonConvert.DeserializeObject<ProductLake>(deletedFileContent);
                return product;
            }

            return NotFound("No such file found");
        }


    }
}
