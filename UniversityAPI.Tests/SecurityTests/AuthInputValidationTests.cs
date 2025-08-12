namespace UniversityAPI.Tests.SecurityTests
{
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.Json;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using UniversityAPI.Framework.Model.User.DTO;
    using UniversityAPI.Tests.Shared.Fixtures;
    using UniversityAPI.Tests.Shared.Helpers;

    public class AuthInputValidationTests(AuthInputValidationTestApplicationFactory factory) : IClassFixture<AuthInputValidationTestApplicationFactory>
    {
        private readonly AuthInputValidationTestApplicationFactory factory = factory;

        [Fact]
        public async Task Register_WeakPassword_ReturnsBadRequest_And_Success_WithStrongPassword()
        {
            var registerDto = new RegisterDto($"testuser_{Guid.NewGuid()}", $"test_{RandomNumberGenerator.GetInt32(0, 9999)}@example.com", "weak");

            var json = JsonSerializer.Serialize(registerDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = this.factory.CreateClient();
            var response = await client.PostAsync("/api/auth/register", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var successfulRegisterDto = registerDto with
            {
                Password = await this.factory.ExecuteScopeAsync(async scope =>
                {
                    var identityOptions = scope.GetRequiredService<IOptions<IdentityOptions>>().Value;
                    return TestPasswordGenerator.GeneratePassword(identityOptions.Password);
                })
            };

            json = JsonSerializer.Serialize(successfulRegisterDto);
            content = new StringContent(json, Encoding.UTF8, "application/json");
            var successResponse = await client.PostAsync("/api/auth/register", content);
            Assert.Equal(HttpStatusCode.OK, successResponse.StatusCode);
        }

        [Fact]
        public async Task Register_InvalidEmail_ReturnsBadRequest()
        {
            var strongPassword = await this.factory.ExecuteScopeAsync(async scope =>
            {
                var identityOptions = scope.GetRequiredService<IOptions<IdentityOptions>>().Value;
                return TestPasswordGenerator.GeneratePassword(identityOptions.Password);
            });

            var registerDto = new RegisterDto("testUser", "invalid-email", strongPassword);

            var json = JsonSerializer.Serialize(registerDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = this.factory.CreateClient();
            var response = await client.PostAsync("/api/auth/register", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_EmptyCredentials_ReturnsBadRequest()
        {
            var loginDto = new LoginDto(string.Empty, string.Empty);

            var json = JsonSerializer.Serialize(loginDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = this.factory.CreateClient();
            var response = await client.PostAsync("/api/auth/login", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Register_EmptyFields_ReturnsBadRequest()
        {
            var registerDto = new RegisterDto(string.Empty, string.Empty, string.Empty);

            var json = JsonSerializer.Serialize(registerDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = this.factory.CreateClient();
            var response = await client.PostAsync("/api/auth/register", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}