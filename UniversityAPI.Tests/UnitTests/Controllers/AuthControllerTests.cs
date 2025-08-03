using Microsoft.AspNetCore.Mvc;
using Moq;
using UniversityAPI.Controllers;
using UniversityAPI.Framework.Model;
using UniversityAPI.Service;

namespace UniversityAPI.Tests.Controllers
{
    public class AuthControllerTests : IClassFixture<UniversityAPITestFixture>
    {
        private readonly UniversityAPITestFixture _fixture;
        private readonly AuthController _authController;
        private readonly Mock<ITokenService> _mockTokenService;

        public AuthControllerTests(UniversityAPITestFixture fixture)
        {
            _fixture = fixture;
            _mockTokenService = new Mock<ITokenService>();
            _authController = new AuthController(_fixture.UserManager, _mockTokenService.Object);
        }

        [Fact]
        public async Task Register_ValidData_ReturnsOkResult()
        {
            var registerDto = new RegisterDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "TestPassword123!"
            };

            var result = await _authController.Register(registerDto);

            Assert.IsType<ActionResult<AuthResponse>>(result);
            Assert.Null(result.Result);
            Assert.NotNull(result.Value);

            var authResponse = result.Value;
            Assert.NotNull(authResponse);
            Assert.Equal(registerDto.Username, authResponse.Username);
            Assert.Equal(registerDto.Email, authResponse.Email);
        }

        [Fact]
        public async Task Register_DuplicateEmail_ReturnsBadRequest()
        {
            var existingUser = _fixture.Context.Users.First();
            var registerDto = new RegisterDto
            {
                Username = "newuser", // Different username
                Email = existingUser.Email, // Duplicate email
                Password = "TestPassword123!"
            };

            var result = await _authController.Register(registerDto);

            Assert.IsType<ActionResult<AuthResponse>>(result);
            Assert.NotNull(result.Result);
            Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task Register_DuplicateUsername_ReturnsBadRequest()
        {
            var existingUser = _fixture.Context.Users.First();
            var registerDto = new RegisterDto
            {
                Username = existingUser.UserName, // Duplicate username
                Email = "different@example.com",  // Different email
                Password = "TestPassword123!"
            };

            var result = await _authController.Register(registerDto);

            Assert.IsType<ActionResult<AuthResponse>>(result);
            Assert.NotNull(result.Result);
            Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkResult()
        {
            // from the seeded test data
            var testUser = new UserDM
            {
                UserName = "testuser@example.com",
                Email = "testuser@example.com",
                EmailConfirmed = true,
                IsActive = true
            };

            var password = "TestUser123!";
            var createResult = await _fixture.UserManager.CreateAsync(testUser, password);
            Assert.True(createResult.Succeeded, "Failed to create test user");

            var loginDto = new LoginDto
            {
                Username = testUser.UserName,
                Password = password
            };

            _mockTokenService.Setup(x => x.GenerateToken(It.IsAny<UserDM>()))
                .ReturnsAsync("mock-jwt-token");

            var result = await _authController.Login(loginDto);

            Assert.IsType<ActionResult<AuthResponse>>(result);
            Assert.Null(result.Result);
            Assert.NotNull(result.Value);

            Assert.Equal(testUser.UserName, result.Value?.Username);
            Assert.Equal(testUser.Email, result.Value?.Email);
            Assert.Equal("mock-jwt-token", result.Value?.Token);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            var loginDto = new LoginDto
            {
                Username = "nonexistent",
                Password = "WrongPassword"
            };

            var result = await _authController.Login(loginDto);
            Assert.IsType<ActionResult<AuthResponse>>(result);
            Assert.NotNull(result.Result);
            Assert.IsType<UnauthorizedResult>(result.Result);
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task Login_EmptyCredentials_ReturnsBadRequest()
        {
            var loginDto = new LoginDto
            {
                Username = "",
                Password = ""
            };
            TestModelValidator.ValidateModel(loginDto, _authController);
            var result = await _authController.Login(loginDto);

            Assert.IsType<ActionResult<AuthResponse>>(result);
            Assert.NotNull(result.Result);
            Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task Register_EmptyData_ReturnsBadRequest()
        {
            var registerDto = new RegisterDto
            {
                Username = "",
                Email = "",
                Password = ""
            };

            TestModelValidator.ValidateModel(registerDto, _authController);

            var result = await _authController.Register(registerDto);

            Assert.IsType<ActionResult<AuthResponse>>(result);
            Assert.NotNull(result.Result);
            Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Null(result.Value);

            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.NotNull(badRequestResult);
        }
    }
}