using System;
using Microsoft.Extensions.DependencyInjection;
using TripStack.TddDemo.CurrencyConverter.Abstractions;

namespace TripStack.TddDemo.CurrencyExchange.ApiClient.DependencyInjection
{
    public static class ApiClientServiceCollectionExtensions
    {
        public static IServiceCollection AddCurrencyExchangeApiClient(this IServiceCollection services, Action<CurrencyExchangeSettings> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }
            
            services.AddOptions();
            services.Configure(setupAction);

            services.AddSingleton<IGetExchangeRates, CurrencyExchangeApiClient>();

            return services;
        }
    }
}