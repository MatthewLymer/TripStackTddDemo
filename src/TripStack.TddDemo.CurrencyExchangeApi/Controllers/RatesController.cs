using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripStack.TddDemo.CurrencyExchangeApi.Models;

namespace TripStack.TddDemo.CurrencyExchangeApi.Controllers
{
    [Authorize]
    [Route("rates")]
    public sealed class RatesController : ControllerBase
    {
        private static readonly Dictionary<string, decimal> UsdConversionRates =
            new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
            {
                {"USD", 1.00m},
                {"CAD", 0.71m},
                {"EUR", 1.10m},
                {"GBP", 1.20m},
                {"MXN", .042m}
            };
        
        [HttpGet]
        public IActionResult GetRates([FromQuery(Name="from")] string fromCurrency, [FromQuery(Name="to")] string toCurrency)
        {
            if (!IsValid(fromCurrency, toCurrency, out var validationErrors))
            {
                return BadRequest(ResponseModel.FromErrors(validationErrors));
            }

            var rateModel = new RateModel
            {
                Value = UsdConversionRates[fromCurrency] / UsdConversionRates[toCurrency]
            };
            
            return Ok(ResponseModel.FromSuccess(rateModel));
        }

        private static bool IsValid(string fromCurrency, string toCurrency, out IEnumerable<ValidationErrorModel> validationErrors)
        {
            var errors = GetValidationErrors(fromCurrency, toCurrency).ToList();
            validationErrors = errors;
            return !errors.Any();
        }

        private static IEnumerable<ValidationErrorModel> GetValidationErrors(string fromCurrency, string toCurrency)
        {
            var message = "Must be one of: " + string.Join(", ", UsdConversionRates.Keys) + ".";
            
            if (fromCurrency == null || !UsdConversionRates.ContainsKey(fromCurrency))
            {
                yield return new ValidationErrorModel
                {
                    Name = "from",
                    Message = message
                };
            }

            if (toCurrency == null || !UsdConversionRates.ContainsKey(toCurrency))
            {
                yield return new ValidationErrorModel
                {
                    Name = "to",
                    Message = message
                };
            }
        }
    }
}