using System.ComponentModel.DataAnnotations;

namespace UniversityAPI.Framework.Model
{
    public class UpdateUniversityDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Country { get; set; }

        [Url]
        public string Webpage { get; set; }
    }
}
