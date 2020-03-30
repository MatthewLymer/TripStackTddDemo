using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TripStack.TddDemo.CurrencyConverter.Abstractions;
using TripStack.TddDemo.WebApi.Models;
using TripStack.TddDemo.WebApi.Persistence;

namespace TripStack.TddDemo.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private const string DefaultCurrencyCode = "USD";
        
        private readonly IGetExchangeRates _exchangeRateGetter;

        public ProductsController(IGetExchangeRates exchangeRateGetter)
        {
            _exchangeRateGetter = exchangeRateGetter;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery(Name="currencyCode")] string currencyCode, CancellationToken token)
        {
            if (string.IsNullOrEmpty(currencyCode))
            {
                currencyCode = DefaultCurrencyCode;
            }

            var productResponseModels = ProductRespository
                .GetProducts()
                .Select(product => new ProductResponseModel {Product = product})
                .ToList();

            foreach (var model in productResponseModels)
            {
                var product = model.Product;
                
                model.ConvertedPrice = product.Price * await _exchangeRateGetter.GetExchangeRateAsync(
                    product.CurrencyCode,
                    currencyCode,
                    token);
            }

            var responseModel = new GetProductsResponseModel
            {
                Data = productResponseModels
            };
            
            return Ok(responseModel);
        }
    }
}