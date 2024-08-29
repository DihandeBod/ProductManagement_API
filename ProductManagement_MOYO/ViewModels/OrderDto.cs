using ProductManagement_MOYO.Models;

namespace ProductManagement_MOYO.ViewModels
{
    public class OrderDto
    {
        public int UserId { get; set; }
        public List<OrderLine> orderLines { get; set; }
    }
}
