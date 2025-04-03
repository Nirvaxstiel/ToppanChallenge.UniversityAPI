using System.ComponentModel.DataAnnotations;

namespace UniversityAPI.Framework.Model
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
