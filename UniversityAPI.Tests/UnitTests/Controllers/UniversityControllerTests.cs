using Microsoft.AspNetCore.Mvc;
using Moq;
using UniversityAPI.Controllers;
using UniversityAPI.Framework.Model;
using UniversityAPI.Service;
using UniversityAPI.Tests.Shared.Fixtures;
using UniversityAPI.Utility;

namespace UniversityAPI.Tests.UnitTests.Controllers
{
    public class UniversityControllerTests : IClassFixture<UnitTestFixture>
    {
        private readonly UnitTestFixture _fixture;
        private readonly UniversityController _universityController;
        private readonly Mock<IUniversityService> _mockUniversityService;
        private readonly Mock<ICurrentUserService> _mockCurrentUserService;

        public UniversityControllerTests(UnitTestFixture fixture)
        {
            _fixture = fixture;
            _mockUniversityService = new Mock<IUniversityService>();
            _mockCurrentUserService = new Mock<ICurrentUserService>();
            _universityController = new UniversityController(_mockUniversityService.Object, _mockCurrentUserService.Object);
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

            _mockUniversityService.Setup(x => x.GetUniversitiesAsync(It.IsAny<Guid>(), It.IsAny<UniversityFilter>(), It.IsAny<PaginationParams>()))
                .ReturnsAsync(mockResult);

            var result = await _universityController.GetUniversities(new UniversityFilter(), new PaginationParams());

            Assert.IsType<ActionResult<PagedResult<UniversityDto>>>(result);
        }

        [Fact]
        public async Task GetUniversity_ExistingId_ReturnsOkResult()
        {
            var universityId = Guid.NewGuid();
            var mockUniversity = new UniversityDto { Id = universityId, Name = "Test University" };

            _mockUniversityService.Setup(x => x.GetUniversityByIdAsync(universityId))
                .ReturnsAsync(mockUniversity);

            var result = await _universityController.GetUniversity(universityId);

            Assert.IsType<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equal(mockUniversity, okResult.Value);
        }

        [Fact]
        public async Task GetUniversity_NonExistentId_ReturnsNotFound()
        {
            var universityId = Guid.NewGuid();

            _mockUniversityService.Setup(x => x.GetUniversityByIdAsync(universityId))
                .ReturnsAsync((UniversityDto)null);

            var result = await _universityController.GetUniversity(universityId);

            Assert.IsType<NotFoundResult>(result.Result);
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task CreateUniversity_ValidData_ReturnsCreatedResult()
        {
            var createDto = new CreateUniversityDto
            {
                Name = "New University",
                Country = "Test Country",
                Webpage = "https://test.edu"
            };

            var mockResult = new CreateUniversityDto
            {
                Name = createDto.Name,
                Country = createDto.Country,
                Webpage = createDto.Webpage
            };

            _mockUniversityService.Setup(x => x.CreateUniversityAsync(It.IsAny<CreateUniversityDto>(), It.IsAny<Guid>()))
                .ReturnsAsync(mockResult);

            var result = await _universityController.CreateUniversity(createDto);

            Assert.IsType<ActionResult<CreateUniversityDto>>(result);
        }

        [Fact]
        public async Task BookmarkUniversity_ValidData_ReturnsOkResult()
        {
            var university = _fixture.Context.Universities.First();
            var user = _fixture.Context.Users.First();

            var mockBookmark = new UserBookmarkDto
            {
                UserId = ConvertHelper.ToGuid(user.Id),
                UniversityId = university.Id
            };

            _mockUniversityService.Setup(x => x.BookmarkUniversityAsync(university.Id, It.IsAny<Guid>()))
                .ReturnsAsync(mockBookmark);

            var result = await _universityController.BookmarkUniversity(university.Id);

            Assert.IsType<NoContentResult>(result);
        }
    }
}