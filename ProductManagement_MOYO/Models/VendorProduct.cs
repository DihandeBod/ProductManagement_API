using System.Text.Json.Serialization;

namespace ProductManagement_MOYO.Models
{
    public class VendorProduct
    {
        public int VendorProductId { get; set; }
        public int VendorId { get; set; }
        public int ProductId { get; set; }
        public double Price { get; set; }
        public int QuantityOnHand { get; set; }
        public int StockLimit { get; set; }
        public bool isActive { get; set; }

        // Navigation
        [JsonIgnore]
        public Product Product { get; set; }
        [JsonIgnore]
        public Vendor Vendor { get; set; }
    }
}
