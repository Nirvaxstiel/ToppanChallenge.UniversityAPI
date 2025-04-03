using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using UniversityAPI.Data;
using UniversityAPI.DataModel;
using UniversityAPI.Services;

namespace UniversityAPI.Tests
{
    public class UniversityServiceTests
    {
        private readonly Mock<ApplicationDbContext> _mockContext;
        private readonly Mock<IMapper> _mockMapper;
        private readonly UniversityService _service;

        public UniversityServiceTests()
        {
            _mockContext = new Mock<ApplicationDbContext>();
            _mockMapper = new Mock<IMapper>();
            _service = new UniversityService(_mockContext.Object, _mockMapper.Object);

            // Setup mock DbSet for Universities
            var universities = new List<UniversityDM>
        {
            new UniversityDM { Id = 1, Name = "Test Uni", Country = "Testland" }
        }.AsQueryable();

            var mockSet = new Mock<DbSet<UniversityDM>>();
            mockSet.As<IQueryable<UniversityDM>>().Setup(m => m.Provider).Returns(universities.Provider);
            mockSet.As<IQueryable<UniversityDM>>().Setup(m => m.Expression).Returns(universities.Expression);
            mockSet.As<IQueryable<UniversityDM>>().Setup(m => m.GetEnumerator()).Returns(universities.GetEnumerator());

            _mockContext.Setup(c => c.Universities).Returns(mockSet.Object);
        }

        [Fact]
        public async Task GetById_ReturnsUniversity_WhenExists()
        {
            var existingId = Guid.NewGuid();
            var testUniversity = new UniversityDM { Id = existingId, Name = "Test Uni" };
            _mockMapper.Setup(m => m.Map<UniversityDto>(It.IsAny<UniversityDM>()))
                .Returns(new UniversityDto { Id = existingId, Name = "Test Uni" });

            var result = await _service.GetUniversityByIdAsync("user1", 1);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Name.Should().Be("Test Uni");
        }
    }
}
