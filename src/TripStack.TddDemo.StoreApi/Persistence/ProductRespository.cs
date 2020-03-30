using System.Collections.Generic;
using System.Linq;
using TripStack.TddDemo.WebApi.Models;

namespace TripStack.TddDemo.WebApi.Persistence
{
    internal static class ProductRespository
    {
        private static readonly IReadOnlyCollection<ProductModel> Products = new []
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

        public static IEnumerable<ProductModel> GetProducts() => Products.ToList().AsReadOnly();
    }
}