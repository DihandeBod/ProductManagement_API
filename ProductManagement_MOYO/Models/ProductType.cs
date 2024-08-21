using System.Text.Json.Serialization;

namespace ProductManagement_MOYO.Models
{
    public class ProductType
    {
        public int ProductTypeId { get; set; }
        public string ProductTypeName { get; set; }
        [JsonIgnore]
        public List<ProductCategory> ProductCategories { get; set; }
    }
}
