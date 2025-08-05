using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using UniversityAPI.Tests.Shared.Models;

namespace UniversityAPI.Tests.Shared.Helpers
{
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly TestAuthOptions testAuthOptions;

        #pragma warning disable SYSLIB0051
        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
                TestAuthOptions testAuthOptions)
            : base(options, logger, encoder, clock)
        {
            this.testAuthOptions = testAuthOptions;
        }
        #pragma warning restore SYSLIB0051

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, testAuthOptions.UserName),
                new Claim(ClaimTypes.NameIdentifier, testAuthOptions.UserId.ToString()),
                new Claim(ClaimTypes.Role, testAuthOptions.Role),
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}