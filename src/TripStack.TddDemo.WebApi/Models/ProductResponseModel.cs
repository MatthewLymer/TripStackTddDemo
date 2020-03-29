namespace TripStack.TddDemo.WebApi.Models
{
    public sealed class ProductResponseModel
    {
        public ProductModel Product { get; set; }
        public decimal ConvertedPrice { get; set; }
    }
}