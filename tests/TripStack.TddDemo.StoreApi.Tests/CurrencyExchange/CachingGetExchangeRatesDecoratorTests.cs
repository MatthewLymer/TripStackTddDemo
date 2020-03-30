using Xunit;

namespace TripStack.TddDemo.StoreApi.Tests.CurrencyExchange
{
    public sealed class CachingGetExchangeRatesDecoratorTests
    {
        [Fact]
        public void ShouldCreateInstance()
        {
            var _ = new CachingGetExchangeRatesDecorator();
        }
    }

    public sealed class CachingGetExchangeRatesDecorator
    {
    }
}