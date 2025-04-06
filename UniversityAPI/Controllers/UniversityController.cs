using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityAPI.Framework.Model;
using UniversityAPI.Service;
using UniversityAPI.Utility;

namespace UniversityAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UniversityController : ControllerBase
    {
        private readonly IUniversityService _universityService;
        private readonly ICurrentUserService _currentUserService;

        public UniversityController(IUniversityService universityService, ICurrentUserService currentUserService)
        {
            _universityService = universityService;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<UniversityDto>>> GetUniversities([FromQuery] UniversityFilter filter, [FromQuery] PaginationParams pagination)
        {
            var result = await _universityService.GetUniversitiesAsync(_currentUserService.UserId, filter, pagination);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UniversityDto>> GetUniversity(Guid id)
        {
            var result = await _universityService.GetUniversityByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CreateUniversityDto>> CreateUniversity(CreateUniversityDto createUniversityDto)
        {
            var result = await _universityService.CreateUniversityAsync(createUniversityDto, _currentUserService.UserId);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUniversity(Guid id, UpdateUniversityDto updateUniversityDto)
        {
            var result = await _universityService.UpdateUniversityAsync(id, updateUniversityDto, _currentUserService.UserId);
            if (result == null)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUniversity(Guid id)
        {
            await _universityService.DeleteUniversityAsync(id, _currentUserService.UserId);
            return NoContent();
        }

        [HttpPost("bookmark/{id}")]
        public async Task<IActionResult> BookmarkUniversity(Guid id)
        {
            var result = await _universityService.BookmarkUniversityAsync(id, _currentUserService.UserId);
            return NoContent();
        }

        [HttpDelete("bookmark/{id}")]
        public async Task<IActionResult> UnbookmarkUniversity(Guid id)
        {
            await _universityService.UnbookmarkUniversityAsync(id, _currentUserService.UserId);
            return NoContent();
        }
    }
}