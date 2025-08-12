namespace UniversityAPI.Framework.Model.University
{
    using UniversityAPI.Framework.Database;
    using UniversityAPI.Framework.Model.User;

    public class UniversityDM : DbModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public string Webpage { get; set; }
        public ICollection<UserBookmarkDM> UserBookmarks { get; set; }
    }
}