using System.Threading;
using System.Threading.Tasks;
using TripStack.TddDemo.CurrencyConverter.Abstractions;
using Xunit;

namespace TripStack.TddDemo.StoreApi.Tests.CurrencyExchange
{
    public sealed class CachingGetExchangeRatesDecoratorTests
    {
        [Fact]
        public void ShouldInheritFromIGetExchangeRates()
        {
            var decorator = new CachingGetExchangeRatesDecorator();
            Assert.IsAssignableFrom<IGetExchangeRates>(decorator);
        }
    }

    public sealed class CachingGetExchangeRatesDecorator : IGetExchangeRates
    {
        public Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency, CancellationToken token)
        {
            throw new System.NotImplementedException();
        }
    }
}