using System.ComponentModel.DataAnnotations;

namespace UniversityAPI.Framework.Model
{
    public class CreateUniversityDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "University name is required")]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Country is required")]
        public string Country { get; set; }

        [Url]
        public string Webpage { get; set; }
    }
}