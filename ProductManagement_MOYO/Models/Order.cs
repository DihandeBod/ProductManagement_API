using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;


namespace ProductManagement_MOYO.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }
        [ForeignKey("UserId")]
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        [ForeignKey("StatusId")]
        public int OrderStatusId { get; set; } = 1;
        public double OrderTotal { get; set; }
        public int? VendorId { get; set; }
        public bool isAssigned { get; set; }

        // Navigation
        [JsonIgnore]
        public OrderStatus OrderStatus { get; set; }
        [JsonIgnore]
        public UserAccount User { get; set; }
        [JsonIgnore]
        public Vendor Vendor { get; set; }
        public List<OrderLine> Lines { get; set; }
    }
}
