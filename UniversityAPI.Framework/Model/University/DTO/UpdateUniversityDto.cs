namespace UniversityAPI.Framework.Model.University.DTO
{
    using System.ComponentModel.DataAnnotations;

    public record UpdateUniversityDto
    {
        public UpdateUniversityDto(string name,
                                      string country,
                                      string webpage)
        {
            Name = name;
            Country = country;
            Webpage = webpage;
        }

        [Required(ErrorMessage = "University name is required")]
        public string Name { get; init; }
        [Required(ErrorMessage = "Country is required")]
        public string Country { get; init; }
        [Url]
        public string Webpage { get; init; }
    }
}