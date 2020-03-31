using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using TripStack.TddDemo.CurrencyConverter.Abstractions;
using Xunit;

namespace TripStack.TddDemo.StoreApi.Tests.CurrencyExchange
{
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
    // 2. Ensure we're able to instantiate the class we want to test
    // 3. Ensure the class implements the interface we're looking to decorate
    // 4. Ensure the decorator returns value from inner service for cache miss
    // 5. Ensure the value can be retrieved from cache
    // 6. Ensure distributed cache is used to share cache between instances
    // 7. Ensure cache expires after 1 minute
    // 
    
    public sealed class CachingGetExchangeRatesDecoratorTests
    {
        [Fact]
        public void ShouldInheritFromIGetExchangeRates()
        {
            var decorator = new CachingGetExchangeRatesDecorator(null, null);
            Assert.IsAssignableFrom<IGetExchangeRates>(decorator);
        }

        [Fact]
        public async Task ShouldReturnInnerServiceValueIfCacheMiss()
        {
            var distributedCache = CreateDistributedCache();
            
            const decimal expectedValue = 5m;
            
            var inner = new MemoryExchangeRateProvider();
            inner.Set("USD", "CAD", expectedValue);
            
            var decorator = new CachingGetExchangeRatesDecorator(inner, distributedCache);

            var result = await decorator.GetExchangeRateAsync("USD", "CAD", CancellationToken.None);
            
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public async Task ShouldCacheRatesUsingDistributedCache()
        {
            var distributedCache = CreateDistributedCache();
            
            var inner = new MemoryExchangeRateProvider();
            inner.Set("EUR", "GBP", 1.1m);
            
            var decorator1 = new CachingGetExchangeRatesDecorator(inner, distributedCache);
            var decorator2 = new CachingGetExchangeRatesDecorator(inner, distributedCache);

            var value1 = await decorator1.GetExchangeRateAsync("EUR", "GBP", CancellationToken.None);
            
            inner.Clear();

            var value2 = await decorator2.GetExchangeRateAsync("EUR", "GBP", CancellationToken.None);
            
            Assert.Equal(value1, value2);
        }

        [Fact]
        public async Task ShouldExpireCacheAfter1Minute()
        {
            var distributedCache = CreateDistributedCache();
            
            var inner = new MemoryExchangeRateProvider();
            inner.Set("CAD", "USD", 2m);
            
            var decorator = new CachingGetExchangeRatesDecorator(inner, distributedCache);

            var value1 = await decorator.GetExchangeRateAsync("CAD", "USD", CancellationToken.None);
            
            inner.Set("CAD", "USD", 4m);

            await Task.Delay(TimeSpan.FromMinutes(1));

            var value2 = await decorator.GetExchangeRateAsync("CAD", "USD", CancellationToken.None);
            
            Assert.Equal(2m, value1);
            Assert.Equal(4m, value2);
        }

        private static MemoryDistributedCache CreateDistributedCache()
        {
            return new MemoryDistributedCache(
                new OptionsWrapper<MemoryDistributedCacheOptions>(new MemoryDistributedCacheOptions()));
        }

        private sealed class MemoryExchangeRateProvider : IGetExchangeRates
        {
            private readonly Dictionary<Tuple<string, string>, decimal> _rates 
                = new Dictionary<Tuple<string, string>, decimal>();
            
            public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency, CancellationToken token)
            {
                await Task.CompletedTask;

                return _rates[Tuple.Create(fromCurrency, toCurrency)];
            }

            public void Set(string fromCurrency, string toCurrency, decimal rate)
            {
                _rates[Tuple.Create(fromCurrency, toCurrency)] = rate;
            }

            public void Clear()
            {
                _rates.Clear();
            }
        }
    }

    internal sealed class CachingGetExchangeRatesDecorator : IGetExchangeRates
    {
        private readonly IGetExchangeRates _inner;
        private readonly IDistributedCache _distributedCache;

        public CachingGetExchangeRatesDecorator(IGetExchangeRates inner, IDistributedCache distributedCache)
        {
            _inner = inner;
            _distributedCache = distributedCache;
        }

        public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency, CancellationToken token)
        {
            var key = $"{fromCurrency}:{toCurrency}";

            var rateString = await _distributedCache.GetStringAsync(key, token);

            if (rateString != null)
            {
                return decimal.Parse(rateString);
            }
            
            var rate = await _inner.GetExchangeRateAsync(fromCurrency, toCurrency, token);

            rateString = rate.ToString(CultureInfo.InvariantCulture);

            var cacheEntryOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
            };
            
            await _distributedCache.SetStringAsync(key, rateString, cacheEntryOptions, CancellationToken.None);
            
            return rate;
        }
    }
}