using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UniversityAPI.Service;
using UniversityAPI.Tests.Shared.Fixtures;

namespace UniversityAPI.Tests.UnitTests.Services
{
    public class TokenServiceTests : IClassFixture<UnitTestFixture>
    {
        private readonly UnitTestFixture _fixture;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly TokenService _tokenService;

        public TokenServiceTests(UnitTestFixture fixture)
        {
            _fixture = fixture;
            _mockConfiguration = new Mock<IConfiguration>();

            // Setup default configuration
            _mockConfiguration.Setup(x => x["TOPPAN_UNIVERSITYAPI_JWT_KEY"])
                .Returns(ConvertHelper.ToBase64String("test-jwt-key-that-is-long-enough-for-hmacsha512"));
            _mockConfiguration.Setup(x => x["Jwt:Issuer"]).Returns("test-issuer");
            _mockConfiguration.Setup(x => x["Jwt:Audience"]).Returns("test-audience");

            _tokenService = new TokenService(_mockConfiguration.Object, _fixture.UserManager);
        }

        [Fact]
        public async Task GenerateToken_ValidUser_ReturnsValidJwtToken()
        {
            var user = _fixture.Context.Users.First();
            if (!await _fixture.UserManager.IsInRoleAsync(user, "User"))
            {
                await _fixture.UserManager.AddToRoleAsync(user, "User");
            }

            var token = await _tokenService.GenerateToken(user);

            Assert.NotNull(token);
            Assert.NotEmpty(token);

            var (principal, validatedToken) = GetPrincipalFromToken(token);

            var jwtToken = (JwtSecurityToken)validatedToken;
            Assert.Equal("test-issuer", jwtToken.Issuer);
            Assert.Equal("test-audience", jwtToken.Audiences.First());
            Assert.True(jwtToken.ValidTo > DateTime.UtcNow);
            Assert.True(jwtToken.ValidFrom <= DateTime.UtcNow);
        }

        [Fact]
        public async Task GenerateToken_UserWithRoles_IncludesRoleClaims()
        {
            var user = _fixture.Context.Users.First();
            // remove roles first
            if (await _fixture.UserManager.IsInRoleAsync(user, "Admin"))
            {
                await _fixture.UserManager.RemoveFromRoleAsync(user, "Admin");
            }
            if (await _fixture.UserManager.IsInRoleAsync(user, "User"))
            {
                await _fixture.UserManager.RemoveFromRoleAsync(user, "User");
            }
            //add role
            await _fixture.UserManager.AddToRoleAsync(user, "Admin");
            await _fixture.UserManager.AddToRoleAsync(user, "User");

            var token = await _tokenService.GenerateToken(user);
            var (principal, validatedToken) = GetPrincipalFromToken(token);

            var roleClaims = principal.FindAll(c => c.Type == ClaimTypes.Role).ToList();
            Assert.Equal(2, roleClaims.Count);
            Assert.Contains(roleClaims, c => c.Value == "Admin");
            Assert.Contains(roleClaims, c => c.Value == "User");
        }

        [Fact]
        public async Task GenerateToken_UserWithoutRoles_ReturnsTokenWithoutRoleClaims()
        {
            var user = _fixture.Context.Users.First();
            var userRoles = await _fixture.UserManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                await _fixture.UserManager.RemoveFromRoleAsync(user, role);
            }

            var token = await _tokenService.GenerateToken(user);
            var (principal, validatedToken) = GetPrincipalFromToken(token);

            var roleClaims = principal.FindAll(c => c.Type == ClaimTypes.Role);
            Assert.Empty(roleClaims);
        }

        [Fact]
        public async Task GenerateToken_IncludesUserClaims()
        {
            var user = _fixture.Context.Users.First();

            var token = await _tokenService.GenerateToken(user);
            var (principal, validatedToken) = GetPrincipalFromToken(token);

            var nameIdentifierClaim = principal.FindFirst(c => c.Type == ClaimTypes.NameIdentifier);
            var nameClaim = principal.FindFirst(c => c.Type == ClaimTypes.Name);
            var emailClaim = principal.FindFirst(c => c.Type == ClaimTypes.Email);

            Assert.NotNull(nameIdentifierClaim);
            Assert.Equal(user.Id.ToString(), nameIdentifierClaim.Value);
            Assert.NotNull(nameClaim);
            Assert.Equal(user.UserName, nameClaim.Value);
            Assert.NotNull(emailClaim);
            Assert.Equal(user.Email, emailClaim.Value);
        }

        [Fact]
        public async Task GenerateToken_MissingJwtKey_ThrowsInvalidOperationException()
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(x => x["TOPPAN_UNIVERSITYAPI_JWT_KEY"]).Returns((string)null);
            mockConfig.Setup(x => x["Jwt:Key"]).Returns((string)null);

            var tokenService = new TokenService(mockConfig.Object, _fixture.UserManager);
            var user = _fixture.Context.Users.First();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                tokenService.GenerateToken(user));

            Assert.Equal("JWT signing key is not configured.", exception.Message);
        }

        [Fact]
        public async Task GenerateToken_EmptyJwtKey_ThrowsInvalidOperationException()
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(x => x["TOPPAN_UNIVERSITYAPI_JWT_KEY"]).Returns("");

            var tokenService = new TokenService(mockConfig.Object, _fixture.UserManager);
            var user = _fixture.Context.Users.First();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                tokenService.GenerateToken(user));

            Assert.Equal("JWT signing key is not configured.", exception.Message);
        }

        [Fact]
        public async Task GenerateToken_EnvironmentVariableFallback_Works()
        {
            var jwtKey = ConvertHelper.ToBase64String("test-jwt-key-that-is-long-enough-for-hmacsha512");
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(x => x["TOPPAN_UNIVERSITYAPI_JWT_KEY"]).Returns((string)null);
            mockConfig.Setup(x => x["Jwt:Key"]).Returns(jwtKey);

            var tokenService = new TokenService(mockConfig.Object, _fixture.UserManager);
            var user = _fixture.Context.Users.First();

            var token = await tokenService.GenerateToken(user);

            Assert.NotNull(token);
            Assert.NotEmpty(token);
        }

        [Fact]
        public async Task GenerateToken_TokenExpiresInOneDay()
        {
            var user = _fixture.Context.Users.First();

            var token = await _tokenService.GenerateToken(user);

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            var expectedExpiry = DateTime.UtcNow.AddDays(1);
            var actualExpiry = jwtToken.ValidTo;

            Assert.True(actualExpiry > DateTime.UtcNow.AddDays(1).AddMinutes(-1));
            Assert.True(actualExpiry < DateTime.UtcNow.AddDays(1).AddMinutes(1));
        }

        private (ClaimsPrincipal, SecurityToken) GetPrincipalFromToken(string token)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_mockConfiguration.Object["TOPPAN_UNIVERSITYAPI_JWT_KEY"]));

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
    }
}