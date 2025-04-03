using Microsoft.EntityFrameworkCore;
using AutoMapper;
using UniversityAPI.Data;
using UniversityAPI.DataModel;
using UniversityAPI.Helpers;
using UniversityAPI.Helpers.Filters;

namespace UniversityAPI.Services
{
    public class UniversityService : IUniversityService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public UniversityService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PagedResult<UniversityDto>> GetUniversitiesAsync(Guid userId, UniversityFilter filter, PaginationParams pagination)
        {
            var query = _context.Universities.Where(u => u.IsActive).AsQueryable();

            if (!string.IsNullOrEmpty(filter.Name))
            {
                query = query.Where(u => u.Name.Contains(filter.Name));
            }

            if (!string.IsNullOrEmpty(filter.Country))
            {
                query = query.Where(u => u.Country.Contains(filter.Country));
            }

            var combinedQuery = query
                .Select(u => new { University = u, IsBookmarked = false });

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

            var result = items.Select(item =>
            {
                var dto = MapHelper.Map<UniversityDM, UniversityDto>(item.University);
                dto.IsBookmarked = item.IsBookmarked;
                return dto;
            }).ToList();

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