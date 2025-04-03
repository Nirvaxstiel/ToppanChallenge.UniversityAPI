using Microsoft.EntityFrameworkCore;
using UniversityAPI.Framework;
using UniversityAPI.Framework.Model;
using UniversityAPI.Utility;

namespace UniversityAPI.Service
{
    public class UniversityService : IUniversityService
    {
        private readonly ApplicationDbContext _context;

        public UniversityService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<UniversityDto>> GetUniversitiesAsync(Guid userId, UniversityFilter filter, PaginationParams pagination)
        {
            var query = _context.Universities.Where(u => u.IsActive).AsQueryable();

            if (!string.IsNullOrEmpty(filter.Name))
            {
                query = query.Where(u => u.Name.ToUpper().Contains(filter.Name.ToUpper()));
            }

            if (!string.IsNullOrEmpty(filter.Country))
            {
                query = query.Where(u => u.Country.ToUpper().Contains(filter.Country.ToUpper()));
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
                dto.IsBookmarked = item.IsBookmarked;
                return dto;
            });

            return new PagedResult<UniversityDto>
            {
                Items = result,
                TotalCount = totalCount,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            };
        }
    }
}