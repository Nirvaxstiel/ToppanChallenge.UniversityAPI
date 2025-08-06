using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using System.Text.Json;
using UniversityAPI.Framework;
using UniversityAPI.Framework.Model;
using UniversityAPI.Tests.Shared.Fixtures;
using UniversityAPI.Tests.Shared.Models;

namespace UniversityAPI.Tests.IntegrationTests
{
    public class AuthorizationTests(ApiTestApplicationFactory factory) : IClassFixture<ApiTestApplicationFactory>
    {
        private readonly ApiTestApplicationFactory factory = factory;

        [Fact]
        public async Task ProtectedEndpoints_WithoutToken_ReturnsUnauthorized()
        {
            var client = this.factory.CreateClient();
            var existingUniversity = await this.factory.ExecuteScopeAsync(async scope =>
            {
                var context = scope.GetRequiredService<ApplicationDbContext>();
                return await context.Universities.FirstOrDefaultAsync();
            });

            var createDto = new CreateUniversityDto
            {
                Name = $"Not empty {Guid.NewGuid()}",
                Country = "Test Country",
                Webpage = "https://test.edu"
            };
            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var createResponse = await client.PostAsync("/api/university", content);
            Assert.Equal(HttpStatusCode.Unauthorized, createResponse.StatusCode);

            var updateResponse = await client.PutAsync($"/api/university/{existingUniversity.Id}", content);
            Assert.Equal(HttpStatusCode.Unauthorized, updateResponse.StatusCode);

            var deleteResponse = await client.DeleteAsync($"/api/university/{existingUniversity.Id}");
            Assert.Equal(HttpStatusCode.Unauthorized, deleteResponse.StatusCode);
        }

        [Fact]
        public async Task ProtectedEndpoints_WithValidToken_ReturnsSuccess()
        {
            var client = this.CreateAuthenticatedClient(role: TestRoleTypes.Admin);
            var createDto = new CreateUniversityDto
            {
                Name = $"Test University {Guid.NewGuid()}",
                Country = "Test Country",
                Webpage = "https://test.edu"
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/university", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ProtectedEndpoints_WithValidToken_InvalidRole_ReturnsForbidden()
        {
            var client = this.CreateAuthenticatedClient();
            var createDto = new CreateUniversityDto
            {
                Name = $"Test University {Guid.NewGuid()}",
                Country = "Test Country",
                Webpage = "https://test.edu"
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/university", content);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        private HttpClient CreateAuthenticatedClient(UserDM userDM = null, string role = null)
        {
            var client = this.factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Test-User", userDM?.UserName ?? "test-user");
            client.DefaultRequestHeaders.Add("X-Test-Role", role ?? "test-role");
            client.DefaultRequestHeaders.Add("X-Test-User-Id", userDM?.Id.ToString() ?? Guid.NewGuid().ToString());
            return client;
        }
    }
}