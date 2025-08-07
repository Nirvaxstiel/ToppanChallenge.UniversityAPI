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
    public class UniversityInputValidationTests(ApiTestApplicationFactory factory) : IClassFixture<ApiTestApplicationFactory>
    {
        private readonly ApiTestApplicationFactory factory = factory;

        [Fact]
        public async Task CreateUniversity_EmptyName_ReturnsBadRequest()
        {
            var createDto = new CreateUniversityDto
            {
                Name = string.Empty,
                Country = $"Test Country {Guid.NewGuid()}",
                Webpage = "https://test.edu"
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = this.CreateAuthenticatedClient(role: TestRoleTypes.Admin);
            var response = await client.PostAsync("/api/university", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateUniversity_NullName_ReturnsBadRequest()
        {
            var createDto = new CreateUniversityDto
            {
                Name = null,
                Country = $"Test Country {Guid.NewGuid()}",
                Webpage = "https://test.edu"
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = this.CreateAuthenticatedClient(role: TestRoleTypes.Admin);
            var response = await client.PostAsync("/api/university", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateUniversity_InvalidWebpageUrl_ReturnsBadRequest()
        {
            var createDto = new CreateUniversityDto
            {
                Name = "Test University",
                Country = $"Test Country {Guid.NewGuid()}",
                Webpage = "not-a-valid-url"
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = this.CreateAuthenticatedClient(role: TestRoleTypes.Admin);
            var response = await client.PostAsync("/api/university", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateUniversity_ExtremelyLongName_ReturnsBadRequest()
        {
            var createDto = new CreateUniversityDto
            {
                Name = new string('A', 1001), // Assuming max length is 1000
                Country = $"Test Country {Guid.NewGuid()}",
                Webpage = "https://test.edu"
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = this.CreateAuthenticatedClient(role: TestRoleTypes.Admin);
            var response = await client.PostAsync("/api/university", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetUniversities_InvalidPagination_ReturnsBadRequest()
        {
            var client = this.CreateAuthenticatedClient(role: TestRoleTypes.Admin);
            var response = await client.GetAsync("/api/university?pageNumber=-1&pageSize=0");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<PagedResult<UniversityDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsType<PagedResult<UniversityDto>>(result);
            Assert.Empty(result.Items);
        }

        [Fact]
        public async Task CreateUniversity_SqlInjectionAttempt_ReturnsBadRequest()
        {
            var createDto = new CreateUniversityDto
            {
                Name = "'; DROP TABLE Universities; --",
                Country = $"Test Country {Guid.NewGuid()}",
                Webpage = "https://test.edu"
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = this.CreateAuthenticatedClient(role: TestRoleTypes.Admin);
            var response = await client.PostAsync("/api/university", content);

            var universities = await this.factory.ExecuteScopeAsync(async scope =>
            {
                var context = scope.GetRequiredService<ApplicationDbContext>();
                return await context.Universities.ToListAsync();
            });

            Assert.True(response.IsSuccessStatusCode);
            Assert.NotEmpty(universities);
            Assert.Contains(universities, item => item.Name == createDto.Name
                                                  && item.Webpage == createDto.Webpage
                                                  && item.Country == createDto.Country);
        }

        [Fact]
        public async Task UpdateUniversity_InvalidIdFormat_ReturnsBadRequest()
        {
            var updateDto = new UpdateUniversityDto
            {
                Name = "Updated Name",
                Country = "Updated Country",
                Webpage = "https://updated.edu"
            };

            var json = JsonSerializer.Serialize(updateDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = this.CreateAuthenticatedClient(role: TestRoleTypes.Admin);
            var response = await client.PutAsync("/api/university/invalid-guid", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetUniversityById_InvalidIdFormat_ReturnsBadRequest()
        {
            var client = this.CreateAuthenticatedClient();
            var response = await client.GetAsync("/api/university/invalid-guid");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateUniversity_EmptyCountry_ReturnsBadRequest()
        {
            var client = this.CreateAuthenticatedClient(role: TestRoleTypes.Admin);
            var createDto = new CreateUniversityDto
            {
                Name = "Test University",
                Country = "",
                Webpage = "https://test.edu"
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/university", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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
