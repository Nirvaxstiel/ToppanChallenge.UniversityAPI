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
    public class UniversitiesController : ControllerBase
    {
        private readonly IUniversityService _universityService;
        private readonly ICurrentUserService _currentUserService;

        public UniversitiesController(IUniversityService universityService, ICurrentUserService currentUserService)
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

        //[HttpGet("{id}")]
        //public async Task<ActionResult<UniversityDto>> GetUniversity(int id)
        //{
        //    var userId = _currentUserService.UserId;
        //    var university = await _universityService.GetUniversityByIdAsync(userId, id);
        //    if (university == null) return NotFound();
        //    return Ok(university);
        //}

        //[HttpPost]
        //[Authorize(Roles = "Admin")]
        //public async Task<ActionResult<UniversityDto>> CreateUniversity(CreateUniversityDto createUniversityDto)
        //{
        //    var university = await _universityService.CreateUniversityAsync(createUniversityDto);
        //    return CreatedAtAction(nameof(GetUniversity), new { id = university.Id }, university);
        //}

        //[HttpPut("{id}")]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> UpdateUniversity(int id, UpdateUniversityDto updateUniversityDto)
        //{
        //    var result = await _universityService.UpdateUniversityAsync(id, updateUniversityDto);
        //    if (result == null)
        //    {
        //        return NotFound();
        //    }

        //    return NoContent();
        //}

        //[HttpDelete("{id}")]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> DeleteUniversity(int id)
        //{
        //    var result = await _universityService.DeleteUniversityAsync(id);
        //    if (!result) return NotFound();
        //    return NoContent();
        //}

        //[HttpPost("bookmark/{id}")]
        //public async Task<IActionResult> BookmarkUniversity(int id)
        //{
        //    var userId = _currentUserService.UserId;
        //    var result = await _universityService.BookmarkUniversityAsync(userId, id);
        //    if (!result) return NotFound();
        //    return NoContent();
        //}

        //[HttpDelete("bookmark/{id}")]
        //public async Task<IActionResult> UnbookmarkUniversity(int id)
        //{
        //    var userId = _currentUserService.UserId;
        //    var result = await _universityService.UnbookmarkUniversityAsync(userId, id);
        //    if (!result) return NotFound();
        //    return NoContent();
        //}
    }
}