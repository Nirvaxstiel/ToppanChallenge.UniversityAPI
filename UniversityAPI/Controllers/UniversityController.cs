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
        private readonly IUniversityService universityService;
        private readonly ICurrentUserService currentUserService;

        public UniversityController(IUniversityService universityService, ICurrentUserService currentUserService)
        {
            this.universityService = universityService;
            this.currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<UniversityDto>>> GetUniversities([FromQuery] UniversityFilter filter, [FromQuery] PaginationParams pagination)
        {
            var result = await universityService.GetUniversitiesAsync(currentUserService.UserId, filter, pagination);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UniversityDto>> GetUniversity(Guid id)
        {
            var result = await universityService.GetUniversityByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CreateUniversityDto>> CreateUniversity(CreateUniversityDto createUniversityDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await universityService.CreateUniversityAsync(createUniversityDto, currentUserService.UserId);
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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await universityService.UpdateUniversityAsync(id, updateUniversityDto, currentUserService.UserId);
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
            await universityService.DeleteUniversityAsync(id, currentUserService.UserId);
            return NoContent();
        }

        [HttpPost("bookmark/{id}")]
        public async Task<IActionResult> BookmarkUniversity(Guid id)
        {
            var result = await universityService.BookmarkUniversityAsync(id, currentUserService.UserId);
            return NoContent();
        }

        [HttpDelete("bookmark/{id}")]
        public async Task<IActionResult> UnbookmarkUniversity(Guid id)
        {
            await universityService.UnbookmarkUniversityAsync(id, currentUserService.UserId);
            return NoContent();
        }
    }
}