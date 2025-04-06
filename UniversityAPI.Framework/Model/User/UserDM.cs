using Microsoft.AspNetCore.Identity;

namespace UniversityAPI.Framework.Model
{
    public class UserDM : IdentityUser
    {
        public virtual ICollection<UserBookmarkDM> BookmarkedUniversities { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
        public bool IsActive { get; set; } = true;
    }
}