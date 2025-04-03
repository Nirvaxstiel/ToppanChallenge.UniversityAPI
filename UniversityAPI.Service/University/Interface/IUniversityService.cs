using UniversityAPI.Framework.Model;
using UniversityAPI.Utility;

namespace UniversityAPI.Service
{
    public interface IUniversityService
    {
        Task<PagedResult<UniversityDto>> GetUniversitiesAsync(Guid userId, UniversityFilter filter, PaginationParams pagination);
    }
}