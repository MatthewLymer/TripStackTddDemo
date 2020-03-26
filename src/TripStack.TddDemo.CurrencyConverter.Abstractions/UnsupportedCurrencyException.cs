using System;

namespace TripStack.TddDemo.CurrencyConverter.Abstractions
{
    public sealed class UnsupportedCurrencyException : Exception
    {
        public UnsupportedCurrencyException(string currencyCode)
            : base($"The currency '{currencyCode}' is not supported.")
        {
        }
    }
}