namespace UniversityAPI.Framework.Model
{
    public class UserBookmarkDM : DbEntity
    {
        public Guid UserId { get; set; }
        public Guid UniversityId { get; set; }
        public virtual UniversityDM University { get; set; }
        public DateTime BookmarkedAt { get; set; } = DateTime.UtcNow;
    }
}