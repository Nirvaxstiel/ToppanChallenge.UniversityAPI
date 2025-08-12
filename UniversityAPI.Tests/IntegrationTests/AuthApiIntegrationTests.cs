namespace UniversityAPI.Tests.IntegrationTests
{
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.Json;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using UniversityAPI.Framework.Database;
    using UniversityAPI.Framework.Model.Authentication.DTO;
    using UniversityAPI.Framework.Model.User.DTO;
    using UniversityAPI.Tests.Shared.Fixtures;
    using UniversityAPI.Tests.Shared.Helpers;
    using UniversityAPI.Utility.Helpers;

    public class AuthApiIntegrationTests(AuthApiTestApplicationFactory factory) : IClassFixture<AuthApiTestApplicationFactory>
    {
        private readonly AuthApiTestApplicationFactory factory = factory;
        private readonly JsonSerializerOptions jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

        [Fact]
        public async Task Register_ValidData_ReturnsCreated()
        {
            var strongPassword = await this.factory.ExecuteScopeAsync(async scope =>
            {
                var identityOptions = scope.GetRequiredService<IOptions<IdentityOptions>>().Value;
                return TestPasswordGenerator.GeneratePassword(identityOptions.Password);
            });

            var registerDto = new RegisterDto($"newuser{RandomNumberGenerator.GetInt32(0, 9999)}@example.com", $"newuser{RandomNumberGenerator.GetInt32(0, 9999)}@example.com", strongPassword);

            var json = JsonSerializer.Serialize(registerDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = this.factory.CreateClient();
            var response = await client.PostAsync("/api/auth/register", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Register_DuplicateEmail_ReturnsConflict()
        {
            var (existingUser, strongPassword) = await this.factory.ExecuteScopeAsync(async scope =>
            {
                var context = scope.GetRequiredService<ApplicationDbContext>();
                var user = await context.Users.FirstOrDefaultAsync();

                var identityOptions = scope.GetRequiredService<IOptions<IdentityOptions>>().Value;
                string strongPassword = TestPasswordGenerator.GeneratePassword(identityOptions.Password);
                return (user, strongPassword);
            });

            var registerDto = new RegisterDto(existingUser.UserName, existingUser.Email, strongPassword);

            var json = JsonSerializer.Serialize(registerDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var client = this.factory.CreateClient();
            var response = await client.PostAsync("/api/auth/register", content);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkWithToken()
        {
            var existingUser = await this.factory.ExecuteScopeAsync(async scope =>
            {
                var context = scope.GetRequiredService<ApplicationDbContext>();
                return await context.Users.FirstOrDefaultAsync();
            });

            // seeded password
            var loginDto = new LoginDto(existingUser.UserName, "Admin123!@@123");

            var json = JsonSerializer.Serialize(loginDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = this.factory.CreateClient();
            var response = await client.PostAsync("/api/auth/login", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent, jsonSerializerOptions);

            Assert.NotNull(authResponse);
            Assert.NotNull(authResponse.Token);
            Assert.NotEmpty(ConvertHelper.ToString(authResponse.Token));
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            var loginDto = new LoginDto(default, default)
            {
                Username = "nonexistentUser",
                Password = "WrongPassword123!@@@"
            };

            var json = JsonSerializer.Serialize(loginDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = this.factory.CreateClient();
            var response = await client.PostAsync("/api/auth/login", content);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}