using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TripStack.TddDemo.CurrencyConverter.Abstractions;
using TripStack.TddDemo.WebApi.Models;

namespace TripStack.TddDemo.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private const string DefaultCurrencyCode = "USD";
        
        private readonly ILogger<ProductsController> _logger;
        private readonly IGetExchangeRates _exchangeRateGetter;

        private readonly IReadOnlyCollection<ProductModel> _products = new []
        {
            new ProductModel
            {
                Name = "Maple Syrup, 250ml",
                CurrencyCode = "CAD",
                Price = 9.99m
            },
            new ProductModel
            {
                Name = "American Cheese, 454g",
                CurrencyCode = "USD",
                Price = 6.95m
            },
            new ProductModel
            {
                Name = "Marmite, 454g",
                CurrencyCode = "GBP",
                Price = 4.99m
            },
            new ProductModel
            {
                Name = "Camembert Cheese, 150g",
                CurrencyCode = "EUR",
                Price = 21.53m
            },
            new ProductModel
            {
                Name = "Prosciutto, 50g",
                CurrencyCode = "EUR",
                Price = 7.29m
            },            
            new ProductModel
            {
                Name = "Avacado, 3-pack",
                CurrencyCode = "MXN",
                Price = 69.47m
            }
        };

        public ProductsController(ILogger<ProductsController> logger, IGetExchangeRates exchangeRateGetter)
        {
            _logger = logger;
            _exchangeRateGetter = exchangeRateGetter;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery(Name="currencyCode")] string currencyCode, CancellationToken token)
        {
            if (string.IsNullOrEmpty(currencyCode))
            {
                currencyCode = DefaultCurrencyCode;
            }

            var productResponseModels = _products
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

            return Ok(new GetProductsResponseModel
            {
                Data = productResponseModels
            });
        }
    }
}