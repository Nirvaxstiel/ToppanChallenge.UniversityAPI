namespace UniversityAPI.Framework.Model.University.DTO
{
    using System.ComponentModel.DataAnnotations;

    public record CreateUniversityDto
    {
        public CreateUniversityDto(Guid id,
                                      string name,
                                      string country,
                                      string webpage)
        {
            Id = id;
            Name = name;
            Country = country;
            Webpage = webpage;
        }

        public Guid Id { get; init; }
        [Required(ErrorMessage = "University name is required")]
        [MaxLength(100)]
        public string Name { get; init; }
        [Required(ErrorMessage = "Country is required")]
        public string Country { get; init; }
        [Url]
        public string Webpage { get; init; }
    }
}