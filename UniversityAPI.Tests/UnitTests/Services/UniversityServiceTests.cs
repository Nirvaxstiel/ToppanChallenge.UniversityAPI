namespace UniversityAPI.Tests.UnitTests.Services
{
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using UniversityAPI.Framework.Model.Exception;
    using UniversityAPI.Framework.Model.University.DTO;
    using UniversityAPI.Service.University;
    using UniversityAPI.Service.User.Interface;
    using UniversityAPI.Tests.UnitTests.Shared;
    using UniversityAPI.Utility.Helpers;
    using UniversityAPI.Utility.Helpers.Filters;

    public class UniversityServiceTests : IClassFixture<UnitTestFixture>
    {
        private readonly UnitTestFixture fixture;
        private readonly Mock<ICurrentUserService> mockCurrentUserService;
        private readonly UniversityService universityService;

        public UniversityServiceTests(UnitTestFixture fixture)
        {
            this.fixture = fixture;
            this.universityService = new UniversityService(this.fixture.Context);
            this.mockCurrentUserService = new Mock<ICurrentUserService>();
            var testUser = this.fixture.Context.Users.First();
            this.mockCurrentUserService.Setup(x => x.UserId).Returns(ConvertHelper.ToGuid(testUser.Id));
        }

        [Fact]
        public async Task CreateUniversity_DuplicateNameAndCountry_ThrowsConflictException()
        {
            var userId = this.mockCurrentUserService.Object.UserId;
            var existingUniversity = this.fixture.Context.Universities.First();
            var duplicateDto = new CreateUniversityDto(Guid.NewGuid(), existingUniversity.Name, existingUniversity.Country, "http://different.edu");

            var exception = await Assert.ThrowsAsync<ConflictError>(() =>
                this.universityService.CreateUniversityAsync(duplicateDto, userId));

            Assert.Equal("A university with this name and country already exists", exception.Message);
        }

        [Fact]
        public async Task UpdateUniversity_NonExistentId_ThrowsNotFoundException()
        {
            var userId = this.mockCurrentUserService.Object.UserId;
            var nonExistentId = Guid.NewGuid();
            var updateDto = new UpdateUniversityDto("Updated Name", "Updated Country", "http://different.edu");

            var exception = await Assert.ThrowsAsync<NotFoundError>(() =>
                this.universityService.UpdateUniversityAsync(nonExistentId, updateDto, userId));

            Assert.Equal("University not found", exception.Message);
        }

        [Fact]
        public async Task BookmarkUniversity_NonExistentUniversity_ThrowsNotFoundException()
        {
            var userId = this.mockCurrentUserService.Object.UserId;
            var nonExistentId = Guid.NewGuid();

            var exception = await Assert.ThrowsAsync<NotFoundError>(() =>
                this.universityService.BookmarkUniversityAsync(nonExistentId, userId));

            Assert.Equal("University not found", exception.Message);
        }

        [Fact]
        public async Task UnbookmarkUniversity_NonExistentBookmark_ThrowsNotFoundException()
        {
            var userId = this.mockCurrentUserService.Object.UserId;
            var university = this.fixture.Context.Universities.First();

            var existingBookmark = this.fixture.Context.UserBookmarks
                .FirstOrDefault(b => b.UniversityId == university.Id && b.UserId == userId);
            if (existingBookmark != null)
            {
                this.fixture.Context.UserBookmarks.Remove(existingBookmark);
                await this.fixture.Context.SaveChangesAsync();
            }

            var exception = await Assert.ThrowsAsync<NotFoundError>(() =>
                this.universityService.UnbookmarkUniversityAsync(university.Id, userId));

            Assert.Equal("Bookmark not found", exception.Message);
        }

        [Fact]
        public async Task DeleteUniversity_NonExistentId_ThrowsNotFoundException()
        {
            var userId = this.mockCurrentUserService.Object.UserId;
            var nonExistentId = Guid.NewGuid();

            var exception = await Assert.ThrowsAsync<NotFoundError>(() =>
                this.universityService.DeleteUniversityAsync(nonExistentId, userId));

            Assert.Equal("University not found", exception.Message);
        }

        [Fact]
        public async Task UpdateUniversity_DuplicateNameAndCountry_ThrowsConflictException()
        {
            var userId = this.mockCurrentUserService.Object.UserId;
            var universities = this.fixture.Context.Universities.Take(2).ToList();
            var universityToUpdate = universities[0];
            var duplicateUniversity = universities[1];

            var updateDto = new UpdateUniversityDto(duplicateUniversity.Name, duplicateUniversity.Country, "http://different.edu");

            var exception = await Assert.ThrowsAsync<ConflictError>(() =>
                this.universityService.UpdateUniversityAsync(universityToUpdate.Id, updateDto, userId));

            Assert.Equal("A university with this name and country already exists", exception.Message);
        }

        [Fact]
        public async Task GetUniversities_EmptyUserId_ReturnsAllWithoutBookmarks()
        {
            var filter = new UniversityFilter();
            var pagination = new PaginationParams { PageNumber = 1, PageSize = 10 };
            var emptyUserId = Guid.Empty;

            var result = await this.universityService.GetUniversitiesAsync(emptyUserId, filter, pagination);

            Assert.NotEmpty(result.Items);
            Assert.All(result.Items, u => Assert.False(u.IsBookmarked));
        }

        [Fact]
        public async Task GetUniversityById_NonExistentId_ReturnsNull()
        {
            var nonExistentId = Guid.NewGuid();

            var result = await this.universityService.GetUniversityByIdAsync(nonExistentId);

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateUniversity_ValidData_Succeeds()
        {
            var userId = this.mockCurrentUserService.Object.UserId;
            var createDto = new CreateUniversityDto(Guid.NewGuid(), $"Test University {Guid.NewGuid()}", $"Test Country {Guid.NewGuid()}", "http://different.edu");

            var result = await this.universityService.CreateUniversityAsync(createDto, userId);

            Assert.NotNull(result);
            Assert.Equal(createDto.Name, result.Name);
            Assert.Equal(createDto.Country, result.Country);
            Assert.Equal(createDto.Webpage, result.Webpage);

            var savedUniversity = this.fixture.Context.Universities.FirstOrDefault(u => u.Name == createDto.Name);
            Assert.NotNull(savedUniversity);
            Assert.True(savedUniversity.IsActive);
        }

        [Fact]
        public async Task UpdateUniversity_ValidData_Succeeds()
        {
            var userId = this.mockCurrentUserService.Object.UserId;
            var existingUniversity = this.fixture.Context.Universities.First();
            var updateDto = new UpdateUniversityDto($"Update University Name {existingUniversity.Name}", $"Update University Country {existingUniversity.Country}", "https://updated.edu");

            var result = await this.universityService.UpdateUniversityAsync(existingUniversity.Id, updateDto, userId);

            Assert.NotNull(result);
            Assert.Equal(updateDto.Name, result.Name);
            Assert.Equal(updateDto.Country, result.Country);
            Assert.Equal(updateDto.Webpage, result.Webpage);

            // Verify it was updated in database
            var updatedUniversity = this.fixture.Context.Universities.First(u => u.Id == existingUniversity.Id);
            Assert.Equal(updateDto.Name, updatedUniversity.Name);
        }

        [Fact]
        public async Task BookmarkUniversity_ValidData_Succeeds()
        {
            var userId = this.mockCurrentUserService.Object.UserId;
            var university = this.fixture.Context.Universities.First();

            var result = await this.universityService.BookmarkUniversityAsync(university.Id, userId);

            Assert.NotNull(result);
            Assert.Equal(university.Id, result.UniversityId);
            Assert.Equal(userId, result.UserId);

            // Verify bookmark was created in database
            var bookmark = this.fixture.Context.UserBookmarks.FirstOrDefault(b =>
                b.UniversityId == university.Id && b.UserId == userId);
            Assert.NotNull(bookmark);
            Assert.True(bookmark.IsActive);
        }

        [Fact]
        public async Task UnbookmarkUniversity_ValidData_Succeeds()
        {
            var userId = this.mockCurrentUserService.Object.UserId;
            var university = this.fixture.Context.Universities.First();

            await this.universityService.BookmarkUniversityAsync(university.Id, userId);
            await this.universityService.UnbookmarkUniversityAsync(university.Id, userId);

            var bookmark = this.fixture.Context.UserBookmarks.IgnoreQueryFilters().FirstOrDefault(b =>
                b.UniversityId == university.Id && b.UserId == userId && !b.IsActive);
            Assert.NotNull(bookmark);
            Assert.False(bookmark.IsActive);
        }

        [Fact]
        public async Task GetUniversities_WithUserId_ReturnsWithBookmarkStatus()
        {
            var userId = this.mockCurrentUserService.Object.UserId;
            var university = this.fixture.Context.Universities.First();

            await this.universityService.BookmarkUniversityAsync(university.Id, userId);

            var filter = new UniversityFilter();
            var pagination = new PaginationParams { PageNumber = 1, PageSize = 10 };

            var result = await this.universityService.GetUniversitiesAsync(userId, filter, pagination);

            Assert.NotEmpty(result.Items);
            var bookmarkedUniversity = result.Items.FirstOrDefault(u => u.Id == university.Id);
            Assert.NotNull(bookmarkedUniversity);
            Assert.True(bookmarkedUniversity.IsBookmarked);
        }

        [Fact]
        public async Task GetUniversityById_ExistingId_ReturnsUniversity()
        {
            var existingUniversity = this.fixture.Context.Universities.First();

            var result = await this.universityService.GetUniversityByIdAsync(existingUniversity.Id);

            Assert.NotNull(result);
            Assert.Equal(existingUniversity.Id, result.Id);
            Assert.Equal(existingUniversity.Name, result.Name);
            Assert.Equal(existingUniversity.Country, result.Country);
        }

        [Fact]
        public async Task DeleteUniversity_ExistingId_Succeeds()
        {
            var userId = this.mockCurrentUserService.Object.UserId;
            var existingUniversity = this.fixture.Context.Universities.First();

            await this.universityService.DeleteUniversityAsync(existingUniversity.Id, userId);

            var deletedUniversity = this.fixture.Context.Universities.IgnoreQueryFilters().First(u => u.Id == existingUniversity.Id);
            Assert.False(deletedUniversity.IsActive);
        }
    }
}