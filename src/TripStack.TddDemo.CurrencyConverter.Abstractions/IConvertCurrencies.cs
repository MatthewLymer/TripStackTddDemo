using System.Threading;
using System.Threading.Tasks;

namespace TripStack.TddDemo.CurrencyConverter.Abstractions
{
    public interface IConvertCurrencies
    {
        Task<decimal> ConvertAsync(string fromCurrency, string toCurrency, CancellationToken token);
    }
}