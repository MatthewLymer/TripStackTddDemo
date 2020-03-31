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
        private static readonly TimeSpan CacheExpiresIn = TimeSpan.FromMinutes(1);
        
        private readonly IGetExchangeRates _inner;
        private readonly IDistributedCache _distributedCache;

        public CachingGetExchangeRatesDecorator(IGetExchangeRates inner, IDistributedCache distributedCache)
        {
            _inner = inner;
            _distributedCache = distributedCache;
        }

        public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency, CancellationToken token)
        {
            var shouldInvert = string.CompareOrdinal(fromCurrency, toCurrency) > 0;

            if (shouldInvert)
            {
                Swap(ref fromCurrency, ref toCurrency);
            }
            
            var key = $"{fromCurrency}:{toCurrency}";

            var rate = await TryGetRateAsync(key, token);

            if (rate == decimal.Zero)
            {
                rate = await _inner.GetExchangeRateAsync(fromCurrency, toCurrency, token);
                await SaveRateAsync(key, rate, token);
            }
            
            if (shouldInvert)
            {
                rate = 1 / rate;
            }

            return rate;
        }

        private async Task<decimal> TryGetRateAsync(string key, CancellationToken token)
        {
            var rateString = await _distributedCache.GetStringAsync(key, token);
            return rateString != null ? decimal.Parse(rateString) : decimal.Zero;
        }

        private Task SaveRateAsync(string key, decimal rate, CancellationToken token)
        {
            var rateString = rate.ToString(CultureInfo.InvariantCulture);

            var cacheEntryOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheExpiresIn
            };

            return _distributedCache.SetStringAsync(key, rateString, cacheEntryOptions, token);
        }

        private static void Swap(ref string a, ref string b)
        {
            var tmp = a;
            a = b;
            b = tmp;
        }
    }
}