using UniversityAPI.Framework.Model;
using UniversityAPI.Utility;

namespace UniversityAPI.Service
{
    public interface IUniversityService
    {
        Task<PagedResult<UniversityDto>> GetUniversitiesAsync(Guid userId, UniversityFilter filter, PaginationParams pagination);

        Task<UniversityDto> GetUniversityByIdAsync(Guid universityId);

        Task<CreateUniversityDto> CreateUniversityAsync(CreateUniversityDto universityDto, Guid createdBy);

        Task<UpdateUniversityDto> UpdateUniversityAsync(Guid universityId, UpdateUniversityDto universityDto, Guid updatedBy);

        Task DeleteUniversityAsync(Guid id, Guid updatedBy);

        Task<UserBookmarkDto> BookmarkUniversityAsync(Guid universityId, Guid userId);

        Task UnbookmarkUniversityAsync(Guid universityId, Guid userId);
    }
}