using UniversityAPI.DataModel;
using UniversityAPI.Helpers;
using UniversityAPI.Helpers.Filters;

namespace UniversityAPI.Services
{
    public interface IUniversityService
    {
        Task<PagedResult<UniversityDto>> GetUniversitiesAsync(Guid userId, UniversityFilter filter, PaginationParams pagination);
    }
}
