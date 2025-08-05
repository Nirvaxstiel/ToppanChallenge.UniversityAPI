using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Validations.Rules;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using UniversityAPI.Framework;
using UniversityAPI.Framework.Model;
using UniversityAPI.Service;
using UniversityAPI.Tests.Shared.Fixtures;
using UniversityAPI.Utility;

namespace UniversityAPI.Tests.IntegrationTests
{
    public class AuthorizationTests(ApiTestApplicationFactory factory) : IClassFixture<ApiTestApplicationFactory>
    {
        private readonly HttpClient client = factory.CreateClient();
        private readonly ApiTestApplicationFactory factory = factory;

        [Fact]
        public async Task Authorization_WithDifferentUserTokens_Isolated()
        {
            var (token1, token2) = await this.factory.ExecuteScopeAsync(async scope =>
            {
                var context = scope.GetRequiredService<ApplicationDbContext>();
                var users = context.Users.Take(2);

                var tokenService = scope.GetRequiredService<ITokenService>();
                var token1 = await tokenService.GenerateToken(users.First());
                var token2 = await tokenService.GenerateToken(users.Last());
                return (token1, token2);
            });

            this.client.DefaultRequestHeaders.Clear();
            this.client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token1}");
            var response1 = await this.client.PostAsync("/api/university", new StringContent("{}", Encoding.UTF8, "application/json"));
            Assert.NotEqual(HttpStatusCode.Unauthorized, response1.StatusCode);

            // User 2 can access with their token
            this.client.DefaultRequestHeaders.Clear();
            this.client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token2}");
            var response2 = await this.client.PostAsync("/api/university", new StringContent("{}", Encoding.UTF8, "application/json"));
            Assert.NotEqual(HttpStatusCode.Unauthorized, response2.StatusCode);
        }

        [Fact]
        public async Task Authorization_WithMalformedToken_ReturnsUnauthorized()
        {
            this.client.DefaultRequestHeaders.Add("Authorization", "Bearer malformed.token.here");
            var response = await this.client.PostAsync("/api/university", new StringContent("{}", Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Authorization_WithoutBearerPrefix_ReturnsUnauthorized()
        {
            var token = await this.factory.ExecuteScopeAsync(async scope =>
            {
                var context = scope.GetRequiredService<ApplicationDbContext>();
                var user = await context.Users.FirstOrDefaultAsync();

                var tokenService = scope.GetRequiredService<ITokenService>();
                return await tokenService.GenerateToken(user);
            });

            this.client.DefaultRequestHeaders.Add("Authorization", token);

            var response = await this.client.PostAsync("/api/university", new StringContent("{}", Encoding.UTF8, "application/json"));

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Authorization_WithWrongTokenFormat_ReturnsUnauthorized()
        {
            var context = this.factory.Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
            this.client.DefaultRequestHeaders.Add("Authorization", "Basic dXNlcjpwYXNz");

            var response = await this.client.PostAsync("/api/university", new StringContent("{}", Encoding.UTF8, "application/json"));

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ProtectedEndpoints_WithExpiredToken_ReturnsUnauthorized()
        {
            var expiredToken = await this.factory.ExecuteScopeAsync(async scope =>
            {
                var context = scope.GetRequiredService<ApplicationDbContext>();
                var user = await context.Users.FirstOrDefaultAsync();
                return CreateExpiredToken(user);
            });

            this.client.DefaultRequestHeaders.Add("Authorization", $"Bearer {expiredToken}");

            var response = await this.client.PostAsync("/api/university", new StringContent("{}", Encoding.UTF8, "application/json"));

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        private string CreateExpiredToken(UserDM user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConvertHelper.ToBase64String("test-jwt-key-that-is-long-enough-for-hmacsha512")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var now = DateTime.UtcNow;
            var issuedAt = now.AddHours(-2);
            var expires = now.AddHours(-1); // already expired

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                IssuedAt = issuedAt,
                NotBefore = issuedAt,
                Expires = expires,
                SigningCredentials = creds,
                Issuer = "test-issuer",
                Audience = "test-audience"
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        [Fact]
        public async Task ProtectedEndpoints_WithInvalidToken_ReturnsUnauthorized()
        {
            var context = this.factory.Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
            this.client.DefaultRequestHeaders.Add("Authorization", "Bearer invalid-token");

            var existingUniversity = context.Universities.First();

            var createResponse = await this.client.PostAsync("/api/university", new StringContent("{}", Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.Unauthorized, createResponse.StatusCode);

            var updateResponse = await this.client.PutAsync($"/api/university/{existingUniversity.Id}", new StringContent("{}", Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.Unauthorized, updateResponse.StatusCode);
        }

        [Fact]
        public async Task ProtectedEndpoints_WithoutToken_ReturnsUnauthorized()
        {
            var context = this.factory.Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var existingUniversity = context.Universities.First();

            var createResponse = await this.client.PostAsync("/api/university", new StringContent("{}", Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.Unauthorized, createResponse.StatusCode);

            var updateResponse = await this.client.PutAsync($"/api/university/{existingUniversity.Id}", new StringContent("{}", Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.Unauthorized, updateResponse.StatusCode);

            var deleteResponse = await this.client.DeleteAsync($"/api/university/{existingUniversity.Id}");
            Assert.Equal(HttpStatusCode.Unauthorized, deleteResponse.StatusCode);
        }

        [Fact]
        public async Task ProtectedEndpoints_WithValidToken_ReturnsSuccess()
        {
            var token = await this.factory.ExecuteScopeAsync(async scope =>
            {
                var context = scope.GetRequiredService<ApplicationDbContext>();
                var user = await context.Users.FirstOrDefaultAsync();

                var tokenService = scope.GetRequiredService<ITokenService>();
                var token = await tokenService.GenerateToken(user);
                return token;
            });
            this.client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var createDto = new CreateUniversityDto
            {
                Name = "Test University",
                Country = "Test Country",
                Webpage = "https://test.edu"
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await this.client.PostAsync("/api/university", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Token_ContainsCorrectClaims()
        {
            var (user, token, jwtKey) = await this.factory.ExecuteScopeAsync(async scope =>
            {
                var context = scope.GetRequiredService<ApplicationDbContext>();
                var user = await context.Users.FirstOrDefaultAsync();

                var userManager = scope.GetRequiredService<UserManager<UserDM>>();
                if (await userManager.IsInRoleAsync(user, "Admin"))
                {
                    await userManager.RemoveFromRoleAsync(user, "Admin");
                }
                await userManager.AddToRoleAsync(user, "Admin");

                var tokenService = scope.GetRequiredService<ITokenService>();
                var token = await tokenService.GenerateToken(user);

                var jwtKey = ConfigHelper.GetJwtKey<string>();
                return (user, token, jwtKey);
            });

            var (principal, validatedToken) = this.GetPrincipalFromToken(token, jwtKey);
            var jwtToken = (JwtSecurityToken)validatedToken;

            Assert.NotNull(principal);
            Assert.NotNull(jwtToken);

            Assert.Equal(user.Id.ToString(), principal.FindFirstValue(ClaimTypes.NameIdentifier));
            Assert.Equal(user.UserName, principal.FindFirstValue(ClaimTypes.Name));
            Assert.Equal(user.Email, principal.FindFirstValue(ClaimTypes.Email));
            Assert.Contains(principal.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        }

        public (ClaimsPrincipal, SecurityToken) GetPrincipalFromToken(string token, string jwtKey)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,

                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,

                NameClaimType = ClaimTypes.Name,
                RoleClaimType = ClaimTypes.Role
            };
            var principal = tokenHandler.ValidateToken(token, validationParams, out SecurityToken validatedToken);
            return (principal, validatedToken);
        }

        [Fact]
        public async Task Token_WithMultipleRoles_ContainsAllRoleClaims()
        {
            var (user, token, jwtKey) = await this.factory.ExecuteScopeAsync(async scope =>
            {
                var context = scope.GetRequiredService<ApplicationDbContext>();
                var user = await context.Users.FirstOrDefaultAsync();

                var userManager = scope.GetRequiredService<UserManager<UserDM>>();
                if (await userManager.IsInRoleAsync(user, "Admin"))
                {
                    await userManager.RemoveFromRoleAsync(user, "Admin");
                }
                if (await userManager.IsInRoleAsync(user, "User"))
                {
                    await userManager.RemoveFromRoleAsync(user, "User");
                }

                await userManager.AddToRoleAsync(user, "Admin");
                await userManager.AddToRoleAsync(user, "User");

                var tokenService = scope.GetRequiredService<ITokenService>();
                var token = await tokenService.GenerateToken(user);

                var jwtKey = ConfigHelper.GetJwtKey<string>();
                return (user, token, jwtKey);
            });

            var (principal, validatedToken) = this.GetPrincipalFromToken(token, jwtKey);

            var roleClaims = principal.FindAll(c => c.Type == ClaimTypes.Role).ToList();
            Assert.Equal(2, roleClaims.Count);
            Assert.Contains(roleClaims, c => c.Value == "Admin");
            Assert.Contains(roleClaims, c => c.Value == "User");
        }

        [Fact]
        public async Task Token_WithoutRoles_ContainsNoRoleClaims()
        {
            var (user, token, jwtKey) = await this.factory.ExecuteScopeAsync(async scope =>
            {
                var context = scope.GetRequiredService<ApplicationDbContext>();
                var user = await context.Users.FirstOrDefaultAsync();

                var userManager = scope.GetRequiredService<UserManager<UserDM>>();
                var userRoles = await userManager.GetRolesAsync(user);
                foreach (var role in userRoles)
                {
                    await userManager.RemoveFromRoleAsync(user, role);
                }

                var tokenService = scope.GetRequiredService<ITokenService>();
                var token = await tokenService.GenerateToken(user);

                var jwtKey = ConfigHelper.GetJwtKey<string>();
                return (user, token, jwtKey);
            });

            var (principal, validatedToken) = this.GetPrincipalFromToken(token, jwtKey);
            var jwtToken = (JwtSecurityToken)validatedToken;

            Assert.NotNull(principal);
            Assert.NotNull(jwtToken);
            Assert.Empty(principal.Claims.Where(c => c.Type == ClaimTypes.Role).ToList());
        }
    }
}