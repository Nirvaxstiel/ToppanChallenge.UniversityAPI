namespace UniversityAPI.Tests.Shared.Helpers
{
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder, clock)
    {

#pragma warning disable SYSLIB0051

#pragma warning restore SYSLIB0051

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var headers = this.Request.Headers;
            var claims = new List<Claim>();

            if (headers.TryGetValue("X-Test-User", out var username))
            {
                claims.Add(new Claim(ClaimTypes.Name, username));
            }

            if (headers.TryGetValue("X-Test-User-Id", out var userId))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            }

            if (headers.TryGetValue("X-Test-Role", out var role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            if (claims.Count == 0)
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var identity = new ClaimsIdentity(claims, this.Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, this.Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}