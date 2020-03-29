using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TripStack.TddDemo.CurrencyExchangeApi.Middleware
{
    internal sealed class BearerAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private const string BearerPrefix = "bearer ";

        private static readonly Dictionary<Guid, string> TokensToTenantNames = new Dictionary<Guid, string>
        {
            {
                Guid.Parse("71FE92A8-E54B-4E99-A130-D960DCD0436E"),
                "acme"
            },
            {
                Guid.Parse("AC72FE70-BC7C-431E-B27C-292BEB6ED350"),
                "globex"
            }
        };
        
        public BearerAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options, 
            ILoggerFactory logger, 
            UrlEncoder encoder, 
            ISystemClock clock) 
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.Headers["WWW-Authenticate"] = "Bearer realm=\"TripStack Currency Converter API\"";
            
            return base.HandleChallengeAsync(properties);
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            await Task.CompletedTask;
            
            if (!Request.Headers.TryGetValue("Authorization", out var authorizationValues))
            {
                return AuthenticateResult.NoResult();
            }

            var authorizationValue = authorizationValues[0];

            if (!authorizationValue.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return AuthenticateResult.Fail("Only bearer authentication is supported.");
            }

            if (!Guid.TryParse(authorizationValue.Substring(BearerPrefix.Length), out var token))
            {
                return AuthenticateResult.Fail("Could not parse bearer token.");
            }

            if (!TokensToTenantNames.TryGetValue(token, out var tenantName))
            {
                return AuthenticateResult.Fail("Given token did not match any tenants.");
            }
            
            var claims = new [] {
                new Claim(ClaimTypes.NameIdentifier, token.ToString()),
                new Claim(ClaimTypes.Name, tenantName),
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);

            var principal = new ClaimsPrincipal(identity); 
            
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            
            return AuthenticateResult.Success(ticket);
        }
    }
}