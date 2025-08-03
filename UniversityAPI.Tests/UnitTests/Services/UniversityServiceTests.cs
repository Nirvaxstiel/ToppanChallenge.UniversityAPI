using Moq;
using UniversityAPI.Framework.Model;
using UniversityAPI.Service;
using UniversityAPI.Utility;

namespace UniversityAPI.Tests.Services
{
    public class UniversityServiceTests : IClassFixture<UniversityAPITestFixture>
    {
        private readonly UniversityAPITestFixture _fixture;
        private readonly Mock<ICurrentUserService> _mockCurrentUserService;
        private readonly UniversityService _universityService;

        public UniversityServiceTests(UniversityAPITestFixture fixture)
        {
            _fixture = fixture;
            _universityService = new UniversityService(_fixture.Context);
            _mockCurrentUserService = new Mock<ICurrentUserService>();
            var testUser = _fixture.Context.Users.First();
            _mockCurrentUserService.Setup(x => x.UserId).Returns(ConvertHelper.ToGuid(testUser.Id));
        }

        [Fact]
        public async Task CreateUniversity_DuplicateNameAndCountry_ThrowsConflictException()
        {
            var userId = _mockCurrentUserService.Object.UserId;
            var existingUniversity = _fixture.Context.Universities.First();
            var duplicateDto = new CreateUniversityDto
            {
                Name = existingUniversity.Name,
                Country = existingUniversity.Country,
                Webpage = "http://different.edu"
            };

            var exception = await Assert.ThrowsAsync<ConflictException>(() =>
                _universityService.CreateUniversityAsync(duplicateDto, userId));

            Assert.Equal("A university with this name and country already exists", exception.Message);
        }

        [Fact]
        public async Task UpdateUniversity_NonExistentId_ThrowsNotFoundException()
        {
            var userId = _mockCurrentUserService.Object.UserId;
            var nonExistentId = Guid.NewGuid();
            var updateDto = new UpdateUniversityDto
            {
                Name = "Updated Name",
                Country = "Updated Country",
                Webpage = "http://updated.edu"
            };

            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _universityService.UpdateUniversityAsync(nonExistentId, updateDto, userId));

            Assert.Equal("University not found", exception.Message);
        }

        [Fact]
        public async Task BookmarkUniversity_NonExistentUniversity_ThrowsNotFoundException()
        {
            var userId = _mockCurrentUserService.Object.UserId;
            var nonExistentId = Guid.NewGuid();

            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _universityService.BookmarkUniversityAsync(nonExistentId, userId));

            Assert.Equal("University not found", exception.Message);
        }

        [Fact]
        public async Task UnbookmarkUniversity_NonExistentBookmark_ThrowsNotFoundException()
        {
            var userId = _mockCurrentUserService.Object.UserId;
            var university = _fixture.Context.Universities.First();

            var existingBookmark = _fixture.Context.UserBookmarks
                .FirstOrDefault(b => b.UniversityId == university.Id && b.UserId == userId);
            if (existingBookmark != null)
            {
                _fixture.Context.UserBookmarks.Remove(existingBookmark);
                await _fixture.Context.SaveChangesAsync();
            }

            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _universityService.UnbookmarkUniversityAsync(university.Id, userId));

            Assert.Equal("Bookmark not found", exception.Message);
        }

        [Fact]
        public async Task DeleteUniversity_NonExistentId_ThrowsNotFoundException()
        {
            var userId = _mockCurrentUserService.Object.UserId;
            var nonExistentId = Guid.NewGuid();

            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _universityService.DeleteUniversityAsync(nonExistentId, userId));

            Assert.Equal("University not found", exception.Message);
        }

        [Fact]
        public async Task UpdateUniversity_DuplicateNameAndCountry_ThrowsConflictException()
        {
            var userId = _mockCurrentUserService.Object.UserId;
            var universities = _fixture.Context.Universities.Take(2).ToList();
            var universityToUpdate = universities[0];
            var duplicateUniversity = universities[1];

            var updateDto = new UpdateUniversityDto
            {
                Name = duplicateUniversity.Name,
                Country = duplicateUniversity.Country,
                Webpage = "http://updated.edu"
            };

            var exception = await Assert.ThrowsAsync<ConflictException>(() =>
                _universityService.UpdateUniversityAsync(universityToUpdate.Id, updateDto, userId));

            Assert.Equal("A university with this name and country already exists", exception.Message);
        }

        [Fact]
        public async Task GetUniversities_EmptyUserId_ReturnsAllWithoutBookmarks()
        {
            var filter = new UniversityFilter();
            var pagination = new PaginationParams { PageNumber = 1, PageSize = 10 };
            var emptyUserId = Guid.Empty;

            var result = await _universityService.GetUniversitiesAsync(emptyUserId, filter, pagination);

            Assert.NotEmpty(result.Items);
            Assert.All(result.Items, u => Assert.False(u.IsBookmarked));
        }

        [Fact]
        public async Task GetUniversityById_NonExistentId_ReturnsNull()
        {
            var nonExistentId = Guid.NewGuid();

            var result = await _universityService.GetUniversityByIdAsync(nonExistentId);

            Assert.Null(result);
        }
    }
}