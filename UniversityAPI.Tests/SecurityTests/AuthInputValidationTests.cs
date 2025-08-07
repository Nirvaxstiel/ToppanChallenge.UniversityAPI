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

namespace UniversityAPI.Tests.SecurityTests
{
    public class AuthInputValidationTests(ApiTestApplicationFactory factory) : IClassFixture<ApiTestApplicationFactory>
    {
        private readonly ApiTestApplicationFactory factory = factory;

        [Fact]
        public async Task Register_WeakPassword_ReturnsBadRequest_And_Success_WithStrongPassword()
        {
            var registerDto = new RegisterDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "weak"
            };

            var json = JsonSerializer.Serialize(registerDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = this.factory.CreateClient();
            var response = await client.PostAsync("/api/auth/register", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            registerDto.Password = await this.factory.ExecuteScopeAsync(async scope =>
            {
                var identityOptions = scope.GetRequiredService<IOptions<IdentityOptions>>().Value;
                return TestPasswordGenerator.GeneratePassword(identityOptions.Password);
            });

            json = JsonSerializer.Serialize(registerDto);
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

            var registerDto = new RegisterDto
            {
                Username = "testuser",
                Email = "invalid-email",
                Password = strongPassword
            };

            var json = JsonSerializer.Serialize(registerDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = this.factory.CreateClient();
            var response = await client.PostAsync("/api/auth/register", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_EmptyCredentials_ReturnsBadRequest()
        {
            var loginDto = new LoginDto
            {
                Username = "",
                Password = ""
            };

            var json = JsonSerializer.Serialize(loginDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = this.factory.CreateClient();
            var response = await client.PostAsync("/api/auth/login", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Register_EmptyFields_ReturnsBadRequest()
        {
            var registerDto = new RegisterDto
            {
                Username = "",
                Email = "",
                Password = ""
            };

            var json = JsonSerializer.Serialize(registerDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = this.factory.CreateClient();
            var response = await client.PostAsync("/api/auth/register", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
