using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TripStack.TddDemo.CurrencyConverter.Abstractions;

namespace TripStack.TddDemo.CurrencyConverter.Memory
{
    internal sealed class SlowMemoryCurrencyConverter : IConvertCurrencies
    {
        private readonly Dictionary<string, decimal> _usdConversionRates =
            new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
            {
                {"USD", 1.00m},
                {"CAD", 0.71m},
                {"EUR", 1.10m},
                {"GBP", 1.20m},
                {"MXN", .042m}
            };
        
        public async Task<decimal> ConvertAsync(string fromCurrency, string toCurrency, CancellationToken token)
        {
            // delay to simulate slow downstream service
            await Task.Delay(TimeSpan.FromSeconds(3), token);
            
            if (string.IsNullOrEmpty(fromCurrency))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(fromCurrency));
            }

            if (string.IsNullOrEmpty(toCurrency))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(toCurrency));
            }

            if (!_usdConversionRates.TryGetValue(fromCurrency, out var fromConversionRate))
            {
                throw new UnsupportedCurrencyException(fromCurrency);
            }

            if (!_usdConversionRates.TryGetValue(toCurrency, out var toConversionRate))
            {
                throw new UnsupportedCurrencyException(toCurrency);
            }

            return fromConversionRate * toConversionRate;
        }
    }
}