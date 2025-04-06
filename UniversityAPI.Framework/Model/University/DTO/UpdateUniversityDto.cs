using System.ComponentModel.DataAnnotations;

namespace UniversityAPI.Framework.Model
{
    public class UpdateUniversityDto
    {
        [Required(ErrorMessage = "University name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Country is required")]
        public string Country { get; set; }

        [Url]
        public string Webpage { get; set; }
    }
}