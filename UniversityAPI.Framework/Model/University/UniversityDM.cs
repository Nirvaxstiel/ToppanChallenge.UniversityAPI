namespace UniversityAPI.Framework.Model
{
    public class UniversityDM : DbEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public string Webpage { get; set; }
        public ICollection<UserBookmarkDM> UserBookmarks { get; set; }
    }
}