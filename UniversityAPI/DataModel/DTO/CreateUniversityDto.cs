using System.ComponentModel.DataAnnotations;

namespace UniversityAPI.DataModel
{
    public class CreateUniversityDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Country { get; set; }

        [Url]
        public string Webpages { get; set; }
    }
}
