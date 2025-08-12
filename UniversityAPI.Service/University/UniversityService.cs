namespace UniversityAPI.Service.University
{
    using Microsoft.EntityFrameworkCore;
    using UniversityAPI.Framework.Database;
    using UniversityAPI.Framework.Model.Exception;
    using UniversityAPI.Framework.Model.University;
    using UniversityAPI.Framework.Model.University.DTO;
    using UniversityAPI.Framework.Model.User;
    using UniversityAPI.Framework.Model.User.DTO;
    using UniversityAPI.Service.University.Interface;
    using UniversityAPI.Utility.Helpers;
    using UniversityAPI.Utility.Helpers.Filters;

    public class UniversityService(ApplicationDbContext context) : IUniversityService
    {
        public async Task<PagedResult<UniversityDto>> GetUniversitiesAsync(Guid userId, UniversityFilter filter, PaginationParams pagination)
        {
            var query = context.Universities.Where(u => u.IsActive).AsQueryable();

            if (!string.IsNullOrEmpty(filter.Name))
            {
                query = query.Where(u => u.Name.Contains(filter.Name, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(filter.Country))
            {
                query = query.Where(u => u.Country.Contains(filter.Country, StringComparison.OrdinalIgnoreCase));
            }

            var combinedQuery = query.Select(u => new { University = u, IsBookmarked = false });

            if (userId != Guid.Empty)
            {
                var bookmarkedQuery = query
                    .Where(u => u.UserBookmarks.Any(ub => ub.UserId == userId))
                    .OrderByDescending(u => u.UserBookmarks.First(ub => ub.UserId == userId).BookmarkedAt);

                var nonBookmarkedQuery = query
                    .Where(u => !u.UserBookmarks.Any(ub => ub.UserId == userId))
                    .OrderBy(u => u.Name);

                combinedQuery = bookmarkedQuery
                    .Select(u => new { University = u, IsBookmarked = true })
                    .Concat(nonBookmarkedQuery.Select(u => new { University = u, IsBookmarked = false }));
            }

            var totalCount = await combinedQuery.CountAsync();
            var items = await combinedQuery
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();

            var result = items.ConvertAll(item =>
            {
                var dto = MapHelper.Map<UniversityDM, UniversityDto>(item.University);
                return dto with { IsBookmarked = item.IsBookmarked };
            });

            return new PagedResult<UniversityDto>
            {
                Items = result,
                TotalCount = totalCount,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            };
        }

        public async Task<UniversityDto> GetUniversityByIdAsync(Guid universityId)
        {
            var universityDM = await context.Universities.FirstOrDefaultAsync(u => u.Id == universityId);
            return MapHelper.Map<UniversityDM, UniversityDto>(universityDM);
        }

        public async Task<CreateUniversityDto> CreateUniversityAsync(CreateUniversityDto universityDto, Guid createdBy)
        {
            var existingUniversity = await context.Universities
                .FirstOrDefaultAsync(u => u.Name == universityDto.Name && u.Country == universityDto.Country);
            if (existingUniversity != null)
            {
                throw new ConflictError("A university with this name and country already exists");
            }

            var universityDM = new UniversityDM
            {
                Id = Guid.NewGuid(),
                Name = universityDto.Name,
                Country = universityDto.Country,
                Webpage = universityDto.Webpage,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            context.Universities.Add(universityDM);
            await context.SaveChangesAsync();

            return MapHelper.Map<UniversityDM, CreateUniversityDto>(universityDM);
        }

        public async Task<UpdateUniversityDto> UpdateUniversityAsync(Guid universityId, UpdateUniversityDto universityDto, Guid updatedBy)
        {
            var universityDM = await context.Universities.FirstOrDefaultAsync(u => u.Id == universityId);
            if (universityDM == null)
            {
                throw new NotFoundError("University not found");
            }

            var existingUniversity = await context.Universities.FirstOrDefaultAsync(u => u.Name == universityDto.Name
                && u.Country == universityDto.Country
                && u.Id != universityId);
            if (existingUniversity != null)
            {
                throw new ConflictError("A university with this name and country already exists");
            }

            universityDM.Name = universityDto.Name;
            universityDM.Country = universityDto.Country;
            universityDM.Webpage = universityDto.Webpage;
            universityDM.UpdatedDate = DateTime.UtcNow;
            universityDM.UpdatedBy = updatedBy;
            context.Universities.Update(universityDM);
            await context.SaveChangesAsync();

            return MapHelper.Map<UniversityDM, UpdateUniversityDto>(universityDM);
        }

        public async Task DeleteUniversityAsync(Guid universityId, Guid updatedBy)
        {
            var universityDM = await context.Universities.FirstOrDefaultAsync(u => u.Id == universityId);
            if (universityDM == null)
            {
                throw new NotFoundError("University not found");
            }

            universityDM.UpdatedDate = DateTime.UtcNow;
            universityDM.UpdatedBy = updatedBy;
            universityDM.IsActive = false;
            context.Universities.Update(universityDM);
            await context.SaveChangesAsync();
        }

        public async Task<UserBookmarkDto> BookmarkUniversityAsync(Guid universityId, Guid userId)
        {
            var universityDM = await context.Universities.FirstOrDefaultAsync(u => u.Id == universityId);
            if (universityDM == null)
            {
                throw new NotFoundError("University not found");
            }

            var bookmarks = context.UserBookmarks.Where(bookmark => bookmark.UserId == userId);
            var bookmark = bookmarks.IgnoreQueryFilters().FirstOrDefault(bookmark => bookmark.UniversityId == universityId);
            if (bookmark == null)
            {
                bookmark = new UserBookmarkDM
                {
                    UserId = userId,
                    UniversityId = universityId,
                    BookmarkedAt = DateTime.UtcNow,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = userId,
                };
                context.UserBookmarks.Add(bookmark);
            }
            else if (bookmark != null && !bookmark.IsActive)
            {
                bookmark.UpdatedDate = DateTime.UtcNow;
                bookmark.UpdatedBy = userId;
                bookmark.IsActive = true;
            }

            await context.SaveChangesAsync();
            return MapHelper.Map<UserBookmarkDM, UserBookmarkDto>(bookmark);
        }

        public async Task UnbookmarkUniversityAsync(Guid universityId, Guid userId)
        {
            var universityDM = await context.Universities.FirstOrDefaultAsync(u => u.Id == universityId);
            if (universityDM == null)
            {
                throw new NotFoundError("University not found");
            }

            var bookmark = await context.UserBookmarks.FirstOrDefaultAsync(bookmark => bookmark.UniversityId == universityId && bookmark.UserId == userId);
            if (bookmark == null)
            {
                throw new NotFoundError("Bookmark not found");
            }

            bookmark.UpdatedDate = DateTime.UtcNow;
            bookmark.UpdatedBy = userId;
            bookmark.IsActive = false;
            context.UserBookmarks.Update(bookmark);
            await context.SaveChangesAsync();
        }
    }
}