namespace ProductManagement_MOYO.ViewModels
{
    public class ProductVM
    {
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public int ProductCategoryId { get; set; }
        public bool IsDeleted { get; set; }
        public bool isApproved { get; set; }
    }
}
