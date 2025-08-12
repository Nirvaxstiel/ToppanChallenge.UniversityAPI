namespace UniversityAPI.Framework.Model.User
{
    using UniversityAPI.Framework.Database;
    using UniversityAPI.Framework.Model.University;

    public class UserBookmarkDM : DbModel
    {
        public Guid UserId { get; set; }
        public Guid UniversityId { get; set; }
        public virtual UniversityDM University { get; set; }
        public DateTime BookmarkedAt { get; set; } = DateTime.UtcNow;
    }
}