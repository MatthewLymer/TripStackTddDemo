using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TripStack.TddDemo.CurrencyConverter.Abstractions;
using Xunit;

namespace TripStack.TddDemo.StoreApi.Tests.CurrencyExchange
{
    public sealed class CachingGetExchangeRatesDecoratorTests
    {
        private readonly MemoryExchangeRateProvider _memoryProvider;
        private readonly CachingGetExchangeRatesDecorator _decorator;

        public CachingGetExchangeRatesDecoratorTests()
        {
            _memoryProvider = new MemoryExchangeRateProvider();
            _decorator = new CachingGetExchangeRatesDecorator(_memoryProvider);
        }
        
        // 
        // Rules of Test Driven Development (http://butunclebob.com/ArticleS.UncleBob.TheThreeRulesOfTdd)
        // 1. You are not allowed to write any production code unless it is to make a failing unit test pass.
        // 2. You are not allowed to write any more of a unit test than is sufficient to fail; and compilation failures are failures.
        // 3. You are not allowed to write any more production code than is sufficient to pass the one failing unit test.
        // 
        // Red, Green, Blue Loop
        // 1. (Red) Write a failing unit test
        // 2. (Green) Make that unit test pass
        // 3. (Blue) Refactor / clean up both production code and tests
        // 
        // Test Cases
        // 1. Ensure unit test setup works 
        // 2. Ensure we can create an instance of the class
        // 3. Ensure the class implements the proper interface
        // 4. Ensure the inner service is used if a cache-miss occurs
        // 

        [Fact]
        public void ShouldInheritFromIGetExchangeRates()
        {
            Assert.IsAssignableFrom<IGetExchangeRates>(_decorator);
        }

        [Fact]
        public async Task ShouldReturnInnerServiceValue()
        {
            const string fromCurrency = "USD";
            const string toCurrency1 = "CAD";
            const string toCurrency2 = "MXN";

            _memoryProvider.Set(fromCurrency, toCurrency1, 1.41m);
            _memoryProvider.Set(fromCurrency, toCurrency2, 23.43m);

            var result1 = await _decorator.GetExchangeRateAsync(fromCurrency, toCurrency1, CancellationToken.None);
            var result2 = await _decorator.GetExchangeRateAsync(fromCurrency, toCurrency2, CancellationToken.None);
            
            Assert.Equal(1.41m, result1);
            Assert.Equal(23.43m, result2);
        }

        [Fact]
        public async Task ShouldReturnSameResultFromCacheHit()
        {
            _memoryProvider.Set("MXN", "CAD", 0.21m);

            var result1 = await _decorator.GetExchangeRateAsync("MXN", "CAD", CancellationToken.None);
            
            _memoryProvider.Set("MXN", "CAD", 999);

            var result2 = await _decorator.GetExchangeRateAsync("MXN", "CAD", CancellationToken.None);
            
            Assert.Equal(0.21m, result1);
            Assert.Equal(0.21m, result2);
        }

        private class MemoryExchangeRateProvider : IGetExchangeRates
        {
            private readonly Dictionary<Tuple<string, string>, decimal> _rates = new Dictionary<Tuple<string, string>, decimal>();

            public Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency, CancellationToken token)
            {
                var rate = _rates[Tuple.Create(fromCurrency, toCurrency)];
                return Task.FromResult(rate);
            }

            public void Set(string fromCurrency, string toCurrency, decimal rate)
            {
                var tuple = Tuple.Create(fromCurrency, toCurrency);
                _rates[tuple] = rate;
            }
        }
    }

    public class CachingGetExchangeRatesDecorator : IGetExchangeRates
    {
        private readonly IGetExchangeRates _inner;
        private readonly Dictionary<string, decimal> _rates = new Dictionary<string, decimal>();

        public CachingGetExchangeRatesDecorator(IGetExchangeRates inner)
        {
            _inner = inner;
        }

        public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency, CancellationToken token)
        {
            var key = $"{fromCurrency}:{toCurrency}";

            if (_rates.TryGetValue(key, out var rate))
            {
                return rate;
            }
            
            rate = await _inner.GetExchangeRateAsync(fromCurrency, toCurrency, token);

            _rates[key] = rate;
            
            return rate;
        }
    }
}