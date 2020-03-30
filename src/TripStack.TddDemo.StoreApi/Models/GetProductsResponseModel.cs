using System.Collections.Generic;

namespace TripStack.TddDemo.WebApi.Models
{
    public sealed class GetProductsResponseModel
    {
        public string Status { get; } = "Success";
        
        public IEnumerable<ProductResponseModel> Data { get; set; }
    }
}