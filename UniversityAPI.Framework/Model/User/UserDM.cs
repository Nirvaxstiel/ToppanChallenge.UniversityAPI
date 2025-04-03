using Microsoft.AspNetCore.Identity;

namespace UniversityAPI.Framework.Model
{
    public class UserDM : IdentityUser
    {
        public ICollection<UserBookmarkDM> BookmarkedUniversities { get; set; }
    }
}