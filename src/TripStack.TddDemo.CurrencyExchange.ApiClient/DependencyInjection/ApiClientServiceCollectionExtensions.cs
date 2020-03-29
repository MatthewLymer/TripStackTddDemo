using System;
using Microsoft.Extensions.DependencyInjection;
using TripStack.TddDemo.CurrencyConverter.Abstractions;

namespace TripStack.TddDemo.CurrencyExchange.ApiClient.DependencyInjection
{
    public static class ApiClientServiceCollectionExtensions
    {
        public static IServiceCollection AddCurrencyExchangeApiClient(this IServiceCollection services, string apiBaseUrl, string authenticationKey)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (string.IsNullOrEmpty(apiBaseUrl))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(apiBaseUrl));
            }

            if (string.IsNullOrEmpty(authenticationKey))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(authenticationKey));
            }

            services.AddSingleton<IGetExchangeRates>(sp => new CurrencyExchangeApiClient(apiBaseUrl, authenticationKey));

            return services;
        }
    }
}