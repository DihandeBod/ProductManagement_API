using System.Text.Json.Serialization;

namespace ProductManagement_MOYO.Models
{
    public class OrderLine
    {
        public int OrderLineId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        // Navigation
        [JsonIgnore]
        public Order? Order { get; set; }
        [JsonIgnore]
        public Product? Product { get; set; }

    }
}
