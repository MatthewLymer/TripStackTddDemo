using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using TripStack.TddDemo.CurrencyConverter.Abstractions;

namespace TripStack.TddDemo.WebApi.CurrencyExchange
{
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