using Microsoft.AspNetCore.Identity;

namespace UniversityAPI.DataModel
{
    public class UserDM : IdentityUser
    {
        public ICollection<UserBookmarkDM> BookmarkedUniversities { get; set; }
    }
}