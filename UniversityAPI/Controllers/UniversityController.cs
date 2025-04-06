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
            var userId = _currentUserService.UserId;
            var result = await _universityService.GetUniversitiesAsync(userId, filter, pagination);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UniversityDto>> GetUniversity(Guid id)
        {
            var userId = _currentUserService.UserId;
            var result = await _universityService.GetUniversityByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CreateUniversityDto>> CreateUniversity(CreateUniversityDto createUniversityDto)
        {
            var userId = _currentUserService.UserId;
            var result = await _universityService.CreateUniversityAsync(createUniversityDto, userId);
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
            var userId = _currentUserService.UserId;
            var result = await _universityService.UpdateUniversityAsync(id, updateUniversityDto, userId);
            if (result == null)
            {
                return NotFound();
            }
            return NoContent();
            //return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUniversity(Guid id)
        {
            var userId = _currentUserService.UserId;
            await _universityService.DeleteUniversityAsync(id, userId);
            return NoContent();
        }

        [HttpPost("bookmark/{id}")]
        public async Task<IActionResult> BookmarkUniversity(Guid id)
        {
            var userId = _currentUserService.UserId;
            var result = await _universityService.BookmarkUniversityAsync(id, userId);
            return NoContent();
        }

        [HttpDelete("bookmark/{id}")]
        public async Task<IActionResult> UnbookmarkUniversity(Guid id)
        {
            var userId = _currentUserService.UserId;
            await _universityService.UnbookmarkUniversityAsync(id, userId);
            return NoContent();
        }
    }
}