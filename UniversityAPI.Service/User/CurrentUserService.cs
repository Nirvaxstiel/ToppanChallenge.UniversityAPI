namespace UniversityAPI.Service.User
{
    using System.Security.Claims;
    using Microsoft.AspNetCore.Http;
    using UniversityAPI.Framework.Model.Exception;
    using UniversityAPI.Service.User.Interface;
    using UniversityAPI.Utility.Helpers;

    public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
    {
        public Guid UserId
        {
            get
            {
                var userId = ConvertHelper.ToGuid(httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier));
                if (userId == Guid.Empty)
                {
                    throw new UnauthorisedError("User ID is missing or invalid");
                }
                return userId;
            }
        }

        public string UserName => httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
    }
}