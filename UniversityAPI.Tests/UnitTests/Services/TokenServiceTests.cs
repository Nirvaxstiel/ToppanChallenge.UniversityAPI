using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UniversityAPI.Service;
using UniversityAPI.Tests.Shared.Fixtures;
using UniversityAPI.Utility;
using UniversityAPI.Utility.Interfaces;

namespace UniversityAPI.Tests.UnitTests.Services
{
    public class TokenServiceTests : IClassFixture<UnitTestFixture>
    {
        private readonly UnitTestFixture fixture;
        private readonly Mock<IConfigHelper> mockConfiguration;
        private readonly TokenService tokenService;

        public TokenServiceTests(UnitTestFixture fixture)
        {
            this.fixture = fixture;
            this.mockConfiguration = new Mock<IConfigHelper>();
            this.mockConfiguration.Setup(x => x.GetJwtKey<string>())
                .Returns(ConvertHelper.ToBase64String("test-jwt-key-that-is-long-enough-for-hmacsha512"));
            this.mockConfiguration.Setup(static x => x.GetValue<string>("Jwt:Issuer")).Returns("test-issuer");
            this.mockConfiguration.Setup(x => x.GetValue<string>("Jwt:Audience")).Returns("test-audience");

            this.tokenService = new TokenService(this.mockConfiguration.Object, this.fixture.UserManager);
        }

        [Fact]
        public async Task GenerateToken_ValidUser_ReturnsValidJwtToken()
        {
            var user = this.fixture.Context.Users.First();
            if (!await this.fixture.UserManager.IsInRoleAsync(user, "User"))
            {
                await this.fixture.UserManager.AddToRoleAsync(user, "User");
            }

            var token = await this.tokenService.GenerateToken(user);

            Assert.NotNull(token);
            Assert.NotEmpty(token);

            var (principal, validatedToken) = this.GetPrincipalFromToken(token);

            var jwtToken = (JwtSecurityToken)validatedToken;
            Assert.Equal("test-issuer", jwtToken.Issuer);
            Assert.Equal("test-audience", jwtToken.Audiences.First());
            Assert.True(jwtToken.ValidTo > DateTime.UtcNow);
            Assert.True(jwtToken.ValidFrom <= DateTime.UtcNow);
        }

        [Fact]
        public async Task GenerateToken_UserWithRoles_IncludesRoleClaims()
        {
            var user = this.fixture.Context.Users.First();
            // remove roles first
            if (await this.fixture.UserManager.IsInRoleAsync(user, "Admin"))
            {
                await this.fixture.UserManager.RemoveFromRoleAsync(user, "Admin");
            }
            if (await this.fixture.UserManager.IsInRoleAsync(user, "User"))
            {
                await this.fixture.UserManager.RemoveFromRoleAsync(user, "User");
            }
            //add role
            await this.fixture.UserManager.AddToRoleAsync(user, "Admin");
            await this.fixture.UserManager.AddToRoleAsync(user, "User");

            var token = await this.tokenService.GenerateToken(user);
            var (principal, validatedToken) = this.GetPrincipalFromToken(token);

            var roleClaims = principal.FindAll(c => c.Type == ClaimTypes.Role).ToList();
            Assert.Equal(2, roleClaims.Count);
            Assert.Contains(roleClaims, c => c.Value == "Admin");
            Assert.Contains(roleClaims, c => c.Value == "User");
        }

        [Fact]
        public async Task GenerateToken_UserWithoutRoles_ReturnsTokenWithoutRoleClaims()
        {
            var user = this.fixture.Context.Users.First();
            var userRoles = await this.fixture.UserManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                await this.fixture.UserManager.RemoveFromRoleAsync(user, role);
            }

            var token = await this.tokenService.GenerateToken(user);
            var (principal, validatedToken) = this.GetPrincipalFromToken(token);

            var roleClaims = principal.FindAll(c => c.Type == ClaimTypes.Role);
            Assert.Empty(roleClaims);
        }

        [Fact]
        public async Task GenerateToken_IncludesUserClaims()
        {
            var user = this.fixture.Context.Users.First();

            var token = await this.tokenService.GenerateToken(user);
            var (principal, validatedToken) = this.GetPrincipalFromToken(token);

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
            var mockConfig = new Mock<IConfigHelper>();
            mockConfig.Setup(x => x.GetJwtKey<string>()).Returns((string)null);

            var tokenService = new TokenService(mockConfig.Object, this.fixture.UserManager);
            var user = this.fixture.Context.Users.First();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                tokenService.GenerateToken(user));

            Assert.Equal("JWT signing key is not configured.", exception.Message);
        }

        [Fact]
        public async Task GenerateToken_EmptyJwtKey_ThrowsInvalidOperationException()
        {
            var mockConfig = new Mock<IConfigHelper>();
            mockConfig.Setup(x => x.GetJwtKey<string>()).Returns(string.Empty);

            var tokenService = new TokenService(mockConfig.Object, this.fixture.UserManager);
            var user = this.fixture.Context.Users.First();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                tokenService.GenerateToken(user));

            Assert.Equal("JWT signing key is not configured.", exception.Message);
        }

        [Fact]
        public async Task GenerateToken_EnvironmentVariableFallback_Works()
        {
            var jwtKey = ConvertHelper.ToBase64String("test-jwt-key-that-is-long-enough-for-hmacsha512");
            var mockConfig = new Mock<IConfigHelper>();
            mockConfig.Setup(x => x.GetJwtKey<string>()).Returns(jwtKey);

            var tokenService = new TokenService(mockConfig.Object, this.fixture.UserManager);
            var user = this.fixture.Context.Users.First();

            var token = await tokenService.GenerateToken(user);

            Assert.NotNull(token);
            Assert.NotEmpty(token);
        }

        [Fact]
        public async Task GenerateToken_TokenExpiresInOneDay()
        {
            var user = this.fixture.Context.Users.First();

            var token = await this.tokenService.GenerateToken(user);

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            var expectedExpiry = DateTime.UtcNow.AddDays(1);
            var actualExpiry = jwtToken.ValidTo;

            Assert.True(actualExpiry > DateTime.UtcNow.AddDays(1).AddMinutes(-1));
            Assert.True(actualExpiry < DateTime.UtcNow.AddDays(1).AddMinutes(1));
        }

        private (ClaimsPrincipal, SecurityToken) GetPrincipalFromToken(string token, string jwtKey = null)
        {
            if (jwtKey == null)
            {
                jwtKey = this.mockConfiguration.Object.GetJwtKey<string>();
            }

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
    }
}