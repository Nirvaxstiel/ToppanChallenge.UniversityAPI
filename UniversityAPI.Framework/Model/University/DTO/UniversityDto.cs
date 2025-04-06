namespace UniversityAPI.Framework.Model
{
    public class UniversityDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public string Webpage { get; set; }
        public bool IsBookmarked { get; set; }
    }
}