using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Newtonsoft.Json;
using TripStack.TddDemo.CurrencyConverter.Abstractions;

namespace TripStack.TddDemo.CurrencyExchange.ApiClient
{
    internal sealed class CurrencyExchangeApiClient : IGetExchangeRates, IDisposable
    {
        private readonly string _apiBaseUrl;
        private readonly string _authorizationKey;

        private readonly HttpClient _httpClient = new HttpClient();

        public CurrencyExchangeApiClient(string apiBaseUrl, string authorizationKey)
        {
            _apiBaseUrl = apiBaseUrl;
            _authorizationKey = authorizationKey;
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency, CancellationToken token)
        {
            if (string.IsNullOrEmpty(fromCurrency))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(fromCurrency));
            }

            if (string.IsNullOrEmpty(toCurrency))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(toCurrency));
            }

            var endpoint = Path.Combine(_apiBaseUrl, "rates").Replace("\\", "/");
            
            var queryBuilder = new QueryBuilder
            {
                {"from", fromCurrency},
                {"to", toCurrency}
            };

            var url = endpoint + queryBuilder.ToQueryString();

            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authorizationKey);

                using (var response = await _httpClient.SendAsync(request, token))
                {
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync();
                    
                    var obj = JsonConvert.DeserializeObject<ResponseModel>(json);

                    return obj.Data.Value;
                }
            }
        }

        private sealed class ResponseModel
        {
            public DataModel Data { get; set; }
        }

        private sealed class DataModel
        {
            public decimal Value { get; set; }
        }
    }
}