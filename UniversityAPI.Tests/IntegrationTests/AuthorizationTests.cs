using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Moq;
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
    public class AuthorizationTests : IClassFixture<ApiTestApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly ApiTestApplicationFactory _factory;

        public AuthorizationTests(ApiTestApplicationFactory factory)
        {
            _client = factory.CreateClient();
            _factory = factory;
        }

        [Fact]
        public async Task Authorization_WithDifferentUserTokens_Isolated()
        {
            var (token1, token2) = await _factory.ExecuteScopeAsync(async scope =>
            {
                var _context = scope.GetRequiredService<ApplicationDbContext>();
                var users = _context.Users.Take(2);

                var _tokenService = scope.GetRequiredService<TokenService>();
                var token1 = await _tokenService.GenerateToken(users.First());
                var token2 = await _tokenService.GenerateToken(users.Last());
                return (token1, token2);
            });

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token1}");
            var response1 = await _client.PostAsync("/api/universities", new StringContent("{}", Encoding.UTF8, "application/json"));
            Assert.NotEqual(HttpStatusCode.Unauthorized, response1.StatusCode);

            // User 2 can access with their token
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token2}");
            var response2 = await _client.PostAsync("/api/universities", new StringContent("{}", Encoding.UTF8, "application/json"));
            Assert.NotEqual(HttpStatusCode.Unauthorized, response2.StatusCode);
        }

        [Fact]
        public async Task Authorization_WithMalformedToken_ReturnsUnauthorized()
        {
            var _context = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _client.DefaultRequestHeaders.Add("Authorization", "Bearer malformed.token.here");

            var response = await _client.PostAsync("/api/universities", new StringContent("{}", Encoding.UTF8, "application/json"));

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Authorization_WithoutBearerPrefix_ReturnsUnauthorized()
        {
            var token = await _factory.ExecuteScopeAsync(async scope =>
            {
                var _context = scope.GetRequiredService<ApplicationDbContext>();
                var user = await _context.Users.FirstOrDefaultAsync();

                var _tokenService = scope.GetRequiredService<TokenService>();
                return await _tokenService.GenerateToken(user);
            });

            _client.DefaultRequestHeaders.Add("Authorization", token);

            var response = await _client.PostAsync("/api/universities", new StringContent("{}", Encoding.UTF8, "application/json"));

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Authorization_WithWrongTokenFormat_ReturnsUnauthorized()
        {
            var _context = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _client.DefaultRequestHeaders.Add("Authorization", "Basic dXNlcjpwYXNz");

            var response = await _client.PostAsync("/api/universities", new StringContent("{}", Encoding.UTF8, "application/json"));

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ProtectedEndpoints_WithExpiredToken_ReturnsUnauthorized()
        {
            var _context = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = _context.Users.First();

            // Create an expired token
            var expiredToken = CreateExpiredToken(user);
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {expiredToken}");

            var response = await _client.PostAsync("/api/universities", new StringContent("{}", Encoding.UTF8, "application/json"));

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ProtectedEndpoints_WithInvalidToken_ReturnsUnauthorized()
        {
            var _context = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _client.DefaultRequestHeaders.Add("Authorization", "Bearer invalid-token");

            var existingUniversity = _context.Universities.First();

            var createResponse = await _client.PostAsync("/api/universities", new StringContent("{}", Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.Unauthorized, createResponse.StatusCode);

            var updateResponse = await _client.PutAsync($"/api/universities/{existingUniversity.Id}", new StringContent("{}", Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.Unauthorized, updateResponse.StatusCode);
        }

        [Fact]
        public async Task ProtectedEndpoints_WithoutToken_ReturnsUnauthorized()
        {
            var _context = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var existingUniversity = _context.Universities.First();

            var createResponse = await _client.PostAsync("/api/universities", new StringContent("{}", Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.Unauthorized, createResponse.StatusCode);

            var updateResponse = await _client.PutAsync($"/api/universities/{existingUniversity.Id}", new StringContent("{}", Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.Unauthorized, updateResponse.StatusCode);

            var deleteResponse = await _client.DeleteAsync($"/api/universities/{existingUniversity.Id}");
            Assert.Equal(HttpStatusCode.Unauthorized, deleteResponse.StatusCode);
        }

        [Fact]
        public async Task ProtectedEndpoints_WithValidToken_ReturnsSuccess()
        {
            var token = await _factory.ExecuteScopeAsync(async scope =>
            {
                var _context = scope.GetRequiredService<ApplicationDbContext>();
                var user = await _context.Users.FirstOrDefaultAsync();

                var _tokenService = scope.GetRequiredService<TokenService>();
                var token = await _tokenService.GenerateToken(user);
                return token;
            });
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var createDto = new CreateUniversityDto
            {
                Name = "Test University",
                Country = "Test Country",
                Webpage = "https://test.edu"
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/universities", content);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task PublicEndpoints_WithoutToken_ReturnsSuccess()
        {
            var _context = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var getUniversitiesResponse = await _client.GetAsync("/api/universities");
            Assert.Equal(HttpStatusCode.OK, getUniversitiesResponse.StatusCode);

            var existingUniversity = _context.Universities.First();
            var getUniversityResponse = await _client.GetAsync($"/api/universities/{existingUniversity.Id}");
            Assert.Equal(HttpStatusCode.OK, getUniversityResponse.StatusCode);
        }

        [Fact]
        public async Task Token_ContainsCorrectClaims()
        {
            var (user, token, jwtKey) = await _factory.ExecuteScopeAsync(async scope =>
            {
                var _context = scope.GetRequiredService<ApplicationDbContext>();
                var user = await _context.Users.FirstOrDefaultAsync();

                var _userManager = scope.GetRequiredService<UserManager<UserDM>>();
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    await _userManager.RemoveFromRoleAsync(user, "Admin");
                }
                await _userManager.AddToRoleAsync(user, "Admin");

                var _tokenService = scope.GetRequiredService<TokenService>();
                var token = await _tokenService.GenerateToken(user);

                var jwtKey = ConfigHelper.GetJwtKey<string>();
                return (user, token, jwtKey);
            });

            var (principal, validatedToken) = GetPrincipalFromToken(token, jwtKey);
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

                ValidateIssuer = false, // or specify issuer if you want to validate it
                ValidateAudience = false, // same with audience
                ValidateLifetime = false, // set to true in production

                NameClaimType = ClaimTypes.Name,
                RoleClaimType = ClaimTypes.Role
            };
            var principal = tokenHandler.ValidateToken(token, validationParams, out SecurityToken validatedToken);
            return (principal, validatedToken);
        }

        [Fact]
        public async Task Token_WithMultipleRoles_ContainsAllRoleClaims()
        {
            var (user, token, jwtKey) = await _factory.ExecuteScopeAsync(async scope =>
            {
                var _context = scope.GetRequiredService<ApplicationDbContext>();
                var user = await _context.Users.FirstOrDefaultAsync();

                var _userManager = scope.GetRequiredService<UserManager<UserDM>>();
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    await _userManager.RemoveFromRoleAsync(user, "Admin");
                }
                if (await _userManager.IsInRoleAsync(user, "User"))
                {
                    await _userManager.RemoveFromRoleAsync(user, "User");
                }

                await _userManager.AddToRoleAsync(user, "Admin");
                await _userManager.AddToRoleAsync(user, "User");

                var _tokenService = scope.GetRequiredService<TokenService>();
                var token = await _tokenService.GenerateToken(user);

                var jwtKey = ConfigHelper.GetJwtKey<string>();
                return (user, token, jwtKey);
            });

            var (principal, validatedToken) = GetPrincipalFromToken(token, jwtKey);

            var roleClaims = principal.FindAll(c => c.Type == ClaimTypes.Role).ToList();
            Assert.Equal(2, roleClaims.Count);
            Assert.Contains(roleClaims, c => c.Value == "Admin");
            Assert.Contains(roleClaims, c => c.Value == "User");
        }

        [Fact]
        public async Task Token_WithoutRoles_ContainsNoRoleClaims()
        {
            var (user, token, jwtKey) = await _factory.ExecuteScopeAsync(async scope =>
            {
                var _context = scope.GetRequiredService<ApplicationDbContext>();
                var user = await _context.Users.FirstOrDefaultAsync();

                var _userManager = scope.GetRequiredService<UserManager<UserDM>>();
                var userRoles = await _userManager.GetRolesAsync(user);
                foreach (var role in userRoles)
                {
                    await _userManager.RemoveFromRoleAsync(user, role);
                }

                var _tokenService = scope.GetRequiredService<TokenService>();
                var token = await _tokenService.GenerateToken(user);

                var jwtKey = ConfigHelper.GetJwtKey<string>();
                return (user, token, jwtKey);
            });

            var (principal, validatedToken) = GetPrincipalFromToken(token, jwtKey);
            var jwtToken = (JwtSecurityToken)validatedToken;

            Assert.NotNull(principal);
            Assert.NotNull(jwtToken);
            Assert.Empty(principal.Claims.Where(c => c.Type == ClaimTypes.Role).ToList());
        }

        private string CreateExpiredToken(UserDM user)
        {
            var configuration = new Mock<IConfiguration>();
            configuration.Setup(x => x["TOPPAN_UNIVERSITYAPI_JWT_KEY"]).Returns("test-jwt-key-that-is-long-enough-for-hmacsha512");
            configuration.Setup(x => x["Jwt:Issuer"]).Returns("test-issuer");
            configuration.Setup(x => x["Jwt:Audience"]).Returns("test-audience");

            var userManager = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<UserManager<UserDM>>();
            var tokenService = new TokenService(configuration.Object, userManager);

            // Create a token that expires in the past
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("test-jwt-key-that-is-long-enough-for-hmacsha512"));
            var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(key, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(-1), // Expired 1 hour ago
                SigningCredentials = creds,
                Issuer = "test-issuer",
                Audience = "test-audience"
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}