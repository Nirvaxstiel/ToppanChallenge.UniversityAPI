namespace UniversityAPI.Tests.SecurityTests
{
    using System.Net;
    using System.Text;
    using System.Text.Json;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using UniversityAPI.Framework.Database;
    using UniversityAPI.Framework.Model.University.DTO;
    using UniversityAPI.Framework.Model.User;
    using UniversityAPI.Tests.Shared.Fixtures;
    using UniversityAPI.Tests.Shared.Models;
    using UniversityAPI.Utility.Helpers;

    public class UniversityInputValidationTests(UniversityInputValidationTestApplicationFactory factory) : IClassFixture<UniversityInputValidationTestApplicationFactory>
    {
        private readonly UniversityInputValidationTestApplicationFactory factory = factory;
        private readonly JsonSerializerOptions jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

        [Fact]
        public async Task CreateUniversity_EmptyName_ReturnsBadRequest()
        {
            var createDto = new CreateUniversityDto(Guid.NewGuid(), string.Empty, $"Test Country {Guid.NewGuid()}", "https://test.edu");

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = this.CreateAuthenticatedClient(role: TestRoleTypes.Admin);
            var response = await client.PostAsync("/api/university", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateUniversity_NullName_ReturnsBadRequest()
        {
            var createDto = new CreateUniversityDto(Guid.NewGuid(), null, $"Test Country {Guid.NewGuid()}", "https://test.edu");

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = this.CreateAuthenticatedClient(role: TestRoleTypes.Admin);
            var response = await client.PostAsync("/api/university", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateUniversity_InvalidWebpageUrl_ReturnsBadRequest()
        {
            var createDto = new CreateUniversityDto(Guid.NewGuid(), "Test University", $"Test Country {Guid.NewGuid()}", "not-a-valid-url");

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = this.CreateAuthenticatedClient(role: TestRoleTypes.Admin);
            var response = await client.PostAsync("/api/university", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateUniversity_ExtremelyLongName_ReturnsBadRequest()
        {
            var createDto = new CreateUniversityDto(Guid.NewGuid(), new string('A', 1001), $"Test Country {Guid.NewGuid()}", "https://test.edu");

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
            var result = JsonSerializer.Deserialize<PagedResult<UniversityDto>>(json, jsonSerializerOptions);

            Assert.IsType<PagedResult<UniversityDto>>(result);
            Assert.Empty(result.Items);
        }

        [Fact]
        public async Task CreateUniversity_SqlInjectionAttempt_ReturnsBadRequest()
        {
            var createDto = new CreateUniversityDto(Guid.NewGuid(), "'; DROP TABLE Universities; --", $"Test Country {Guid.NewGuid()}", "https://test.edu");

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
            var updateDto = new UpdateUniversityDto("Updated Name", "Updated Country", "https://updated.edu");

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
            var createDto = new CreateUniversityDto(Guid.NewGuid(), "Test University", "", "https://test.edu");

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