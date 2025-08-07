using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;
using System.Text.Json;
using UniversityAPI.Framework;
using UniversityAPI.Framework.Model;
using UniversityAPI.Tests.Shared.Fixtures;
using UniversityAPI.Tests.Shared.Helpers;
using UniversityAPI.Tests.Shared.Models;
using UniversityAPI.Utility;

namespace UniversityAPI.Tests.IntegrationTests
{
    public class AuthApiIntegrationTests(ApiTestApplicationFactory factory) : IClassFixture<ApiTestApplicationFactory>
    {
        private readonly ApiTestApplicationFactory factory = factory;

        [Fact]
        public async Task Register_ValidData_ReturnsCreated()
        {
            var strongPassword = await this.factory.ExecuteScopeAsync(async scope =>
            {
                var identityOptions = scope.GetRequiredService<IOptions<IdentityOptions>>().Value;
                return TestPasswordGenerator.GeneratePassword(identityOptions.Password);
            });

            var registerDto = new RegisterDto
            {
                Username = "newuser@example.com",
                Email = "newuser@example.com",
                Password = strongPassword,
            };

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

            var registerDto = new RegisterDto
            {
                Username = existingUser.UserName,
                Email = existingUser.Email,
                Password = strongPassword,
            };

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

            var loginDto = new LoginDto
            {
                Username = existingUser.UserName,
                Password = "Admin123!@@123" // seeded password
            };

            var json = JsonSerializer.Serialize(loginDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = this.factory.CreateClient();
            var response = await client.PostAsync("/api/auth/login", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(authResponse);
            Assert.NotNull(authResponse.Token);
            Assert.NotEmpty(ConvertHelper.ToString(authResponse.Token));
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            var loginDto = new LoginDto
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
