using System.ComponentModel.DataAnnotations;

namespace ProductManagement_MOYO.Models
{
    public class ProductLake
    {
        [Key]
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsApproved { get; set; }

        public int ProductCategoryId { get; set; }
    }
}
