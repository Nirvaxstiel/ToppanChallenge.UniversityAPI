namespace UniversityAPI.Tests.UnitTests.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using UniversityAPI.Controllers;
    using UniversityAPI.Framework.Model.University.DTO;
    using UniversityAPI.Framework.Model.User.DTO;
    using UniversityAPI.Service.University.Interface;
    using UniversityAPI.Service.User.Interface;
    using UniversityAPI.Tests.Shared.Fixtures;
    using UniversityAPI.Utility.Helpers;
    using UniversityAPI.Utility.Helpers.Filters;

    public class UniversityControllerTests : IClassFixture<UnitTestFixture>
    {
        private readonly UnitTestFixture fixture;
        private readonly UniversityController universityController;
        private readonly Mock<IUniversityService> mockUniversityService;
        private readonly Mock<ICurrentUserService> mockCurrentUserService;

        public UniversityControllerTests(UnitTestFixture fixture)
        {
            this.fixture = fixture;
            this.mockUniversityService = new Mock<IUniversityService>();
            this.mockCurrentUserService = new Mock<ICurrentUserService>();
            this.universityController = new UniversityController(this.mockUniversityService.Object, this.mockCurrentUserService.Object);
        }

        [Fact]
        public async Task GetUniversities_ReturnsOkResult()
        {
            var mockResult = new PagedResult<UniversityDto>
            {
                Items = new List<UniversityDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            };

            this.mockUniversityService.Setup(x => x.GetUniversitiesAsync(It.IsAny<Guid>(), It.IsAny<UniversityFilter>(), It.IsAny<PaginationParams>()))
                .ReturnsAsync(mockResult);

            var result = await this.universityController.GetUniversities(new UniversityFilter(), new PaginationParams());

            Assert.IsType<ActionResult<PagedResult<UniversityDto>>>(result);
        }

        [Fact]
        public async Task GetUniversity_ExistingId_ReturnsOkResult()
        {
            var universityId = Guid.NewGuid();
            var mockUniversity = new UniversityDto(universityId, $"Test University {Guid.NewGuid()}", $"Test Country {Guid.NewGuid()}", $"https://test.edu/{Guid.NewGuid()}", true);

            this.mockUniversityService.Setup(x => x.GetUniversityByIdAsync(universityId))
                .ReturnsAsync(mockUniversity);

            var result = await this.universityController.GetUniversity(universityId);

            Assert.IsType<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equal(mockUniversity, okResult.Value);
        }

        [Fact]
        public async Task GetUniversity_NonExistentId_ReturnsNotFound()
        {
            var universityId = Guid.NewGuid();

            mockUniversityService.Setup(x => x.GetUniversityByIdAsync(universityId))
                .ReturnsAsync((UniversityDto)null);

            var result = await this.universityController.GetUniversity(universityId);

            Assert.IsType<NotFoundResult>(result.Result);
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task CreateUniversity_ValidData_ReturnsCreatedResult()
        {
            var createDto = new CreateUniversityDto(Guid.NewGuid(), $"New University {Guid.NewGuid()}", $"Test Country {Guid.NewGuid()}", "https://test.edu");

            var mockResult = new CreateUniversityDto(Guid.NewGuid(), createDto.Name, createDto.Country, createDto.Webpage);

            this.mockUniversityService.Setup(x => x.CreateUniversityAsync(It.IsAny<CreateUniversityDto>(), It.IsAny<Guid>()))
                .ReturnsAsync(mockResult);

            var result = await this.universityController.CreateUniversity(createDto);

            Assert.IsType<ActionResult<CreateUniversityDto>>(result);
        }

        [Fact]
        public async Task BookmarkUniversity_ValidData_ReturnsOkResult()
        {
            var university = this.fixture.Context.Universities.First();
            var user = this.fixture.Context.Users.First();

            var mockBookmark = new UserBookmarkDto(ConvertHelper.ToGuid(user.Id), university.Id);

            this.mockUniversityService.Setup(x => x.BookmarkUniversityAsync(university.Id, It.IsAny<Guid>()))
                .ReturnsAsync(mockBookmark);

            var result = await this.universityController.BookmarkUniversity(university.Id);

            Assert.IsType<NoContentResult>(result);
        }
    }
}