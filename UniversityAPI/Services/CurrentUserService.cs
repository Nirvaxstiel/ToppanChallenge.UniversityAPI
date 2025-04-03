using System.Security.Claims;

namespace UniversityAPI.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid UserId
        {
            get
            {
                var userId = ConvertHelper.ToGuid(_httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier));
                if (userId == Guid.Empty)
                {
                    throw new InvalidOperationException("User ID is missing or invalid");
                }
                return userId;
            }
        }

        public string UserName => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
    }
}