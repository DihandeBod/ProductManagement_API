namespace ProductManagement_MOYO.ViewModels
{
    public class VendorConfigureDto
    {
        public int VendorId { get; set; }
        public int ProductId { get; set; }
        public double Price { get; set; }
        public int stockCount { get; set; }
        public int stockLimit { get; set; }
    }
}
