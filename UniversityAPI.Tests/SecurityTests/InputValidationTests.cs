using System.Net;
using System.Text;
using System.Text.Json;
using UniversityAPI.Framework.Model;
using UniversityAPI.Tests.Shared.Fixtures;

namespace UniversityAPI.Tests.SecurityTests
{
    public class InputValidationTests(ApiTestApplicationFactory factory) : IClassFixture<ApiTestApplicationFactory>
    {
        private readonly HttpClient client = factory.CreateClient();
        private readonly ApiTestApplicationFactory factory = factory;

        [Fact]
        public async Task CreateUniversity_EmptyName_ReturnsBadRequest()
        {
            var createDto = new CreateUniversityDto
            {
                Name = string.Empty,
                Country = "Test Country",
                Webpage = "https://test.edu"
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await this.client.PostAsync("/api/university", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateUniversity_NullName_ReturnsBadRequest()
        {
            var createDto = new CreateUniversityDto
            {
                Name = null,
                Country = "Test Country",
                Webpage = "https://test.edu"
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await this.client.PostAsync("/api/university", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateUniversity_InvalidWebpageUrl_ReturnsBadRequest()
        {
            var createDto = new CreateUniversityDto
            {
                Name = "Test University",
                Country = "Test Country",
                Webpage = "not-a-valid-url"
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await this.client.PostAsync("/api/university", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateUniversity_ExtremelyLongName_ReturnsBadRequest()
        {
            var createDto = new CreateUniversityDto
            {
                Name = new string('A', 1001), // Assuming max length is 1000
                Country = "Test Country",
                Webpage = "https://test.edu"
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await this.client.PostAsync("/api/university", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Register_WeakPassword_ReturnsBadRequest()
        {
            var registerDto = new RegisterDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "weak"
            };

            var json = JsonSerializer.Serialize(registerDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await this.client.PostAsync("/api/auth/register", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Register_InvalidEmail_ReturnsBadRequest()
        {
            var registerDto = new RegisterDto
            {
                Username = "testuser",
                Email = "invalid-email",
                Password = "ValidPassword123!"
            };

            var json = JsonSerializer.Serialize(registerDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await this.client.PostAsync("/api/auth/register", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Register_PasswordMismatch_ReturnsBadRequest()
        {
            var registerDto = new RegisterDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "ValidPassword123!"
            };

            var json = JsonSerializer.Serialize(registerDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await this.client.PostAsync("/api/auth/register", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetUniversities_InvalidPagination_ReturnsBadRequest()
        {
            var response = await this.client.GetAsync("/api/university?pageNumber=-1&pageSize=0");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetUniversities_ExtremelyLargePageSize_ReturnsBadRequest()
        {
            var response = await this.client.GetAsync("/api/university?pageSize=10001");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateUniversity_SqlInjectionAttempt_ReturnsBadRequest()
        {
            var createDto = new CreateUniversityDto
            {
                Name = "'; DROP TABLE Universities; --",
                Country = "Test Country",
                Webpage = "https://test.edu"
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await this.client.PostAsync("/api/university", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateUniversity_XssAttempt_ReturnsBadRequest()
        {
            var createDto = new CreateUniversityDto
            {
                Name = "<script>alert('xss')</script>",
                Country = "Test Country",
                Webpage = "https://test.edu"
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await this.client.PostAsync("/api/university", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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

            var response = await this.client.PutAsync("/api/university/invalid-guid", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetUniversityById_InvalidIdFormat_ReturnsBadRequest()
        {
            var response = await this.client.GetAsync("/api/university/invalid-guid");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateUniversity_EmptyCountry_ReturnsBadRequest()
        {
            var createDto = new CreateUniversityDto
            {
                Name = "Test University",
                Country = "",
                Webpage = "https://test.edu"
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await this.client.PostAsync("/api/university", content);

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

            var response = await this.client.PostAsync("/api/auth/login", content);

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

            var response = await this.client.PostAsync("/api/auth/register", content);

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