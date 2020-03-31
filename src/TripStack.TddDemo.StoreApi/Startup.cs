using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TripStack.TddDemo.CurrencyConverter.Abstractions;
using TripStack.TddDemo.CurrencyExchange.ApiClient.DependencyInjection;
using TripStack.TddDemo.WebApi.CurrencyExchange;
using TripStack.TddDemo.WebApi.Settings;

namespace TripStack.TddDemo.WebApi
{
    internal sealed class Startup
    {
        private readonly IConfiguration _configuration;
        
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddStackExchangeRedisCache(x =>
            {
                var settings = _configuration.GetSection("Redis").Get<RedisSettings>();
                x.Configuration = $"{settings.Host}:{settings.Port}";
            });

            services.AddCurrencyExchangeApiClient(x =>
            {
                var settings = _configuration.GetSection("CurrencyExchange").Get<CurrencyExchangeSettings>();
                x.Url = settings.Url;
                x.ApiKey = settings.ApiKey;
            });

            services.Decorate<IGetExchangeRates, CachingGetExchangeRatesDecorator>();
        }
        
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}