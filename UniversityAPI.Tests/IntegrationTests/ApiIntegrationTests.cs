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
    public class ApiIntegrationTests(ApiTestApplicationFactory factory) : IClassFixture<ApiTestApplicationFactory>
    {
        private readonly ApiTestApplicationFactory factory = factory;

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
            var result = JsonSerializer.Deserialize<PagedResult<UniversityDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

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
            var university = JsonSerializer.Deserialize<UniversityDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

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

            var createDto = new CreateUniversityDto
            {
                Name = "Test University",
                Country = "Test Country",
                Webpage = "https://test.edu"
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = this.CreateAuthenticatedClient(role: TestRoleTypes.Admin);
            var response = await client.PostAsync("/api/university", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            var createdUniversity = JsonSerializer.Deserialize<UniversityDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

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

            var createDto = new CreateUniversityDto
            {
                Name = existingUniversity.Name,
                Country = existingUniversity.Country,
                Webpage = "https://different.edu"
            };

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
            var updateDto = new UpdateUniversityDto
            {
                Name = "Updated University Name",
                Country = "Updated Country",
                Webpage = "https://updated.edu"
            };

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
            var updateDto = new UpdateUniversityDto
            {
                Name = "Updated Name",
                Country = "Updated Country",
                Webpage = "https://updated.edu"
            };

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