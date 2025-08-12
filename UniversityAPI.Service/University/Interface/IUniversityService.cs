namespace UniversityAPI.Service.University.Interface
{
    using UniversityAPI.Framework.Model.University.DTO;
    using UniversityAPI.Framework.Model.User.DTO;
    using UniversityAPI.Utility.Helpers;
    using UniversityAPI.Utility.Helpers.Filters;

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