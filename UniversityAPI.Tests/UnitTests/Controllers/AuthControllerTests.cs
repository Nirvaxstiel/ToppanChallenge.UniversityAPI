namespace UniversityAPI.Tests.UnitTests.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using UniversityAPI.Controllers;
    using UniversityAPI.Framework.Model.Authentication.DTO;
    using UniversityAPI.Framework.Model.User;
    using UniversityAPI.Framework.Model.User.DTO;
    using UniversityAPI.Service.Authentication.Interface;
    using UniversityAPI.Tests.Shared.Fixtures;
    using UniversityAPI.Tests.Shared.Helpers;

    public class AuthControllerTests : IClassFixture<UnitTestFixture>
    {
        private readonly UnitTestFixture fixture;
        private readonly AuthController authController;
        private readonly Mock<ITokenService> mockTokenService;

        public AuthControllerTests(UnitTestFixture fixture)
        {
            this.fixture = fixture;
            this.mockTokenService = new Mock<ITokenService>();
            this.authController = new AuthController(this.fixture.UserManager, this.mockTokenService.Object);
        }

        [Fact]
        public async Task Register_ValidData_ReturnsOkResult()
        {
            var strongPassword = TestPasswordGenerator.GeneratePassword(this.fixture.UserManager.Options.Password);
            var registerDto = new RegisterDto("testuser", "test@example.com", strongPassword);

            var result = await this.authController.Register(registerDto);

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
            var existingUser = this.fixture.Context.Users.First();
            var strongPassword = TestPasswordGenerator.GeneratePassword(this.fixture.UserManager.Options.Password);

            var registerDto = new RegisterDto("newuser", existingUser?.Email, strongPassword);

            var result = await this.authController.Register(registerDto);

            Assert.IsType<ActionResult<AuthResponse>>(result);
            Assert.NotNull(result.Result);
            Assert.IsType<ConflictObjectResult>(result.Result);
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task Register_DuplicateUsername_ReturnsBadRequest()
        {
            var existingUser = this.fixture.Context.Users.First();
            var strongPassword = TestPasswordGenerator.GeneratePassword(this.fixture.UserManager.Options.Password);

            var registerDto = new RegisterDto(existingUser?.UserName, "different@example.com", strongPassword);

            var result = await this.authController.Register(registerDto);

            Assert.IsType<ActionResult<AuthResponse>>(result);
            Assert.NotNull(result.Result);
            Assert.IsType<ConflictObjectResult>(result.Result);
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

            var password = "TestUser123!@@123"; // seeded password
            var createResult = await this.fixture.UserManager.CreateAsync(testUser, password);
            Assert.True(createResult.Succeeded, "Failed to create test user");

            var loginDto = new LoginDto(testUser.UserName, password);

            this.mockTokenService.Setup(x => x.GenerateToken(It.IsAny<UserDM>()))
                .ReturnsAsync("mock-jwt-token");

            var result = await this.authController.Login(loginDto);

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
            var strongPassword = TestPasswordGenerator.GeneratePassword(this.fixture.UserManager.Options.Password);
            var loginDto = new LoginDto("nonexistent", strongPassword);
            var result = await this.authController.Login(loginDto);
            Assert.IsType<ActionResult<AuthResponse>>(result);
            Assert.NotNull(result.Result);
            Assert.IsType<UnauthorizedResult>(result.Result);
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task Login_EmptyCredentials_ReturnsBadRequest()
        {
            var loginDto = new LoginDto(string.Empty, string.Empty);
            TestModelValidator.ValidateModel(loginDto, this.authController);
            var result = await this.authController.Login(loginDto);

            Assert.IsType<ActionResult<AuthResponse>>(result);
            Assert.NotNull(result.Result);
            Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task Register_EmptyData_ReturnsBadRequest()
        {
            var registerDto = new RegisterDto(string.Empty, string.Empty, string.Empty);

            TestModelValidator.ValidateModel(registerDto, this.authController);

            var result = await this.authController.Register(registerDto);

            Assert.IsType<ActionResult<AuthResponse>>(result);
            Assert.NotNull(result.Result);
            Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Null(result.Value);

            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.NotNull(badRequestResult);
        }
    }
}