using System.Text.Json.Serialization;

namespace ProductManagement_MOYO.Models
{
    public class ProductCategory
    {
        public int ProductCategoryId { get; set; }
        public string ProductCategoryName { get; set; }

        public int ProductTypeId { get; set; }

        public ProductType ProductType { get; set; }

        [JsonIgnore]
        public List<Product> Products { get; set; }
    }
}
