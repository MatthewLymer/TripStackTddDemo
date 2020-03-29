using System.Threading;
using System.Threading.Tasks;

namespace TripStack.TddDemo.CurrencyConverter.Abstractions
{
    public interface IGetExchangeRates
    {
        Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency, CancellationToken token);
    }
}