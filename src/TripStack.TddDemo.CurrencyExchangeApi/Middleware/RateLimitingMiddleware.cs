using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TripStack.TddDemo.CurrencyExchangeApi.Middleware
{
    internal sealed class RateLimitingMiddleware
    {
        private const int MaxInvocationsPerPeriod = 15;
        private static readonly TimeSpan SamplePeriod = TimeSpan.FromMinutes(1);
        private static readonly object Sync = new object();
        private static readonly Dictionary<string, int> InvocationCounts = new Dictionary<string, int>();
        private static readonly Stopwatch PeriodStopwatch = Stopwatch.StartNew();

        private readonly RequestDelegate _next;

        public RateLimitingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public async Task InvokeAsync(HttpContext context)
        {
            var identityName = context.User?.Identity?.Name;

            if (identityName == null)
            {
                await _next(context);
                return;
            }

            int invocationCount;

            lock (Sync)
            {
                if (PeriodStopwatch.Elapsed >= SamplePeriod)
                {
                    InvocationCounts.Clear();
                    PeriodStopwatch.Restart();
                }

                if (!InvocationCounts.TryGetValue(identityName, out invocationCount))
                {
                    invocationCount = 0;
                }

                invocationCount++;
                
                InvocationCounts[identityName] = invocationCount;
            }

            var resetSeconds = (int) (SamplePeriod - PeriodStopwatch.Elapsed).TotalSeconds;

            var responseHeaders = context.Response.Headers;
            responseHeaders.Add("X-RateLimit-Limit", MaxInvocationsPerPeriod.ToString());
            responseHeaders.Add("X-RateLimit-Remaining", Math.Max(0, MaxInvocationsPerPeriod - invocationCount).ToString());            
            responseHeaders.Add("X-RateLimit-Reset", resetSeconds.ToString(CultureInfo.InvariantCulture));
            
            if (invocationCount > MaxInvocationsPerPeriod)
            {
                context.Response.StatusCode = (int) HttpStatusCode.TooManyRequests;
                return;
            }

            await _next(context);
        }
    }
}