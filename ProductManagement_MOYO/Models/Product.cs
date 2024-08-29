using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ProductManagement_MOYO.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsApproved { get; set; }
        
        public int ProductCategoryId { get; set; }
        // Navigation properties
        [JsonIgnore]
        public ProductCategory ProductCategory { get; set; }
        [JsonIgnore]
        public List<OrderLine> OrderLines { get; set; }
        public List<VendorProduct> VendorProducts { get; set; }
    }
}
