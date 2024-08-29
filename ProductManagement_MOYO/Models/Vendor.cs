using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ProductManagement_MOYO.Models
{
    public class Vendor
    {
        public int VendorId { get; set; }
        [ForeignKey("UserId")]
        public int UserId { get; set; }
        public string VendorName { get; set; }

        // Navigation
        [JsonIgnore]
        public UserAccount UserAccount { get; set; }
        public List<VendorProduct> Products { get; set; }
        public List<Order> Orders { get; set; }
    }
}
