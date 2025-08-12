namespace UniversityAPI.Tests.IntegrationTests
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

    public class UniversityApiIntegrationTests(UniversityApiTestApplicationFactory factory) : IClassFixture<UniversityApiTestApplicationFactory>
    {
        private readonly UniversityApiTestApplicationFactory factory = factory;
        private readonly JsonSerializerOptions jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

        [Fact]
        public async Task GetUniversities_ReturnsSuccessStatusCode()
        {
            var client = this.CreateAuthenticatedClient();
            var response = await client.GetAsync("/api/university");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(content);
        }

        [Fact]
        public async Task GetUniversities_WithPagination_ReturnsCorrectData()
        {
            var client = this.CreateAuthenticatedClient();
            var response = await client.GetAsync("/api/university?pageNumber=1&pageSize=5");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<PagedResult<UniversityDto>>(content, jsonSerializerOptions);

            Assert.NotNull(result);
            Assert.True(result.Items.Count <= 5);
            Assert.True(result.TotalCount > 0);
        }

        [Fact]
        public async Task GetUniversityById_ExistingId_ReturnsUniversity()
        {
            var existingUniversity = await this.factory.ExecuteScopeAsync(async scope =>
            {
                var context = scope.GetRequiredService<ApplicationDbContext>();
                return await context.Universities.FirstOrDefaultAsync();
            });
            var client = this.CreateAuthenticatedClient();
            var response = await client.GetAsync($"/api/university/{existingUniversity.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var university = JsonSerializer.Deserialize<UniversityDto>(content, jsonSerializerOptions);

            Assert.NotNull(university);
            Assert.Equal(existingUniversity.Id, university.Id);
            Assert.Equal(existingUniversity.Name, university.Name);
        }

        [Fact]
        public async Task GetUniversityById_NonExistentId_ReturnsNotFound()
        {
            var nonExistentId = Guid.NewGuid();
            var client = this.CreateAuthenticatedClient();
            var response = await client.GetAsync($"/api/university/{nonExistentId}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateUniversity_ValidData_ReturnsCreated()
        {
            var existingUniversity = await this.factory.ExecuteScopeAsync(async scope =>
            {
                var context = scope.GetRequiredService<ApplicationDbContext>();
                return await context.Universities.FirstOrDefaultAsync();
            });

            var createDto = new CreateUniversityDto(Guid.NewGuid(), $"Test University {Guid.NewGuid}", $"Test Country  {Guid.NewGuid}", "https://test.edu");

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = this.CreateAuthenticatedClient(role: TestRoleTypes.Admin);
            var response = await client.PostAsync("/api/university", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            var createdUniversity = JsonSerializer.Deserialize<UniversityDto>(responseContent, jsonSerializerOptions);

            Assert.NotNull(createdUniversity);
            Assert.Equal(createDto.Name, createdUniversity.Name);
            Assert.Equal(createDto.Country, createdUniversity.Country);
        }

        [Fact]
        public async Task CreateUniversity_DuplicateNameAndCountry_ReturnsConflict()
        {
            var existingUniversity = await this.factory.ExecuteScopeAsync(async scope =>
            {
                var context = scope.GetRequiredService<ApplicationDbContext>();
                return await context.Universities.FirstOrDefaultAsync();
            });

            var createDto = new CreateUniversityDto(Guid.NewGuid(), existingUniversity.Name, existingUniversity.Country, "https://test.edu");

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = this.CreateAuthenticatedClient(role: TestRoleTypes.Admin);
            var response = await client.PostAsync("/api/university", content);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task UpdateUniversity_ValidData_ReturnsOk()
        {
            var existingUniversity = await this.factory.ExecuteScopeAsync(async scope =>
            {
                var context = scope.GetRequiredService<ApplicationDbContext>();
                return await context.Universities.FirstOrDefaultAsync();
            });
            var updateDto = new UpdateUniversityDto($"Updated Name {existingUniversity.Name}", $"Updated Country {existingUniversity.Country}", "https://updated.edu");

            var json = JsonSerializer.Serialize(updateDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = this.CreateAuthenticatedClient(role: TestRoleTypes.Admin);
            var response = await client.PutAsync($"/api/university/{existingUniversity.Id}", content);

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task UpdateUniversity_NonExistentId_ReturnsNotFound()
        {
            var nonExistentId = Guid.NewGuid();
            var updateDto = new UpdateUniversityDto("Updated Name", "Updated Country", "https://updated.edu");

            var json = JsonSerializer.Serialize(updateDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = this.CreateAuthenticatedClient(role: TestRoleTypes.Admin);
            var response = await client.PutAsync($"/api/university/{nonExistentId}", content);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteUniversity_ExistingId_ReturnsNoContent()
        {
            var existingUniversity = await this.factory.ExecuteScopeAsync(async scope =>
            {
                var context = scope.GetRequiredService<ApplicationDbContext>();
                return await context.Universities.FirstOrDefaultAsync();
            });
            var client = this.CreateAuthenticatedClient(role: TestRoleTypes.Admin);
            var response = await client.DeleteAsync($"/api/university/{existingUniversity.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteUniversity_NonExistentId_ReturnsNotFound()
        {
            var nonExistentId = Guid.NewGuid();
            var client = this.CreateAuthenticatedClient(role: TestRoleTypes.Admin);
            var response = await client.DeleteAsync($"/api/university/{nonExistentId}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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