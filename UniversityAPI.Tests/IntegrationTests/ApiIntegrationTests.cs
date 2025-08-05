using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UniversityAPI.Framework;
using UniversityAPI.Framework.Model;
using UniversityAPI.Tests.Shared.Fixtures;
using UniversityAPI.Tests.Shared.Helpers;
using UniversityAPI.Utility;

namespace UniversityAPI.Tests.IntegrationTests
{
    public class ApiIntegrationTests : IClassFixture<ApiTestApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly ApiTestApplicationFactory _factory;

        public ApiIntegrationTests(ApiTestApplicationFactory factory)
        {
            _client = factory.CreateClient();
            _factory = factory;
        }

        [Fact]
        public async Task GetUniversities_ReturnsSuccessStatusCode()
        {
            var response = await _client.GetAsync("/api/universities");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(content);
        }

        [Fact]
        public async Task GetUniversities_WithPagination_ReturnsCorrectData()
        {
            var response = await _client.GetAsync("/api/universities?pageNumber=1&pageSize=5");

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
            var existingUniversity = await _factory.ExecuteScopeAsync(async scope =>
            {
                var context = scope.GetRequiredService<ApplicationDbContext>();
                return await context.Universities.FirstOrDefaultAsync();
            });

            var response = await _client.GetAsync($"/api/universities/{existingUniversity.Id}");

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

            var response = await _client.GetAsync($"/api/universities/{nonExistentId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateUniversity_ValidData_ReturnsCreated()
        {
            var createDto = new CreateUniversityDto
            {
                Name = "Test University",
                Country = "Test Country",
                Webpage = "https://test.edu"
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/universities", content);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
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
            var existingUniversity = await _factory.ExecuteScopeAsync(async scope =>
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

            var response = await _client.PostAsync("/api/universities", content);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task UpdateUniversity_ValidData_ReturnsOk()
        {
            var existingUniversity = await _factory.ExecuteScopeAsync(async scope =>
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

            var response = await _client.PutAsync($"/api/universities/{existingUniversity.Id}", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            var updatedUniversity = JsonSerializer.Deserialize<UniversityDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(updatedUniversity);
            Assert.Equal(updateDto.Name, updatedUniversity.Name);
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

            var response = await _client.PutAsync($"/api/universities/{nonExistentId}", content);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteUniversity_ExistingId_ReturnsNoContent()
        {
            var existingUniversity = await _factory.ExecuteScopeAsync(async scope =>
            {
                var context = scope.GetRequiredService<ApplicationDbContext>();
                return await context.Universities.FirstOrDefaultAsync();
            });

            var response = await _client.DeleteAsync($"/api/universities/{existingUniversity.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteUniversity_NonExistentId_ReturnsNotFound()
        {
            var nonExistentId = Guid.NewGuid();

            var response = await _client.DeleteAsync($"/api/universities/{nonExistentId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Register_ValidData_ReturnsCreated()
        {
            var registerDto = new RegisterDto
            {
                Email = "newuser@example.com",
                Password = "NewUser123!",
            };

            var json = JsonSerializer.Serialize(registerDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/auth/register", content);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Register_DuplicateEmail_ReturnsConflict()
        {
            var existingUser = await _factory.ExecuteScopeAsync(async scope =>
            {
                var context = scope.GetRequiredService<ApplicationDbContext>();
                return await context.Users.FirstOrDefaultAsync();
            });
            var registerDto = new RegisterDto
            {
                Email = existingUser.Email,
                Password = "Test123!",
            };

            var json = JsonSerializer.Serialize(registerDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/auth/register", content);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkWithToken()
        {
            var existingUser = await _factory.ExecuteScopeAsync(async scope =>
            {
                var context = scope.GetRequiredService<ApplicationDbContext>();
                return await context.Users.FirstOrDefaultAsync();
            });
            var loginDto = new LoginDto
            {
                Username = existingUser.UserName,
                Password = "TestUser1!"
            };

            var json = JsonSerializer.Serialize(loginDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/auth/login", content);

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
                Password = "WrongPassword123!"
            };

            var json = JsonSerializer.Serialize(loginDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/auth/login", content);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        private async Task<(ApplicationDbContext context, RoleManager<IdentityRole> roleManager, UserManager<UserDM> userManager)> SetupAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserDM>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            await TestDataSeeder.SeedDataAsync(context, roleManager, userManager);
            return (context, roleManager, userManager);
        }
    }
}