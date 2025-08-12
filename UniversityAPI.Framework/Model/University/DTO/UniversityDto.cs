namespace UniversityAPI.Framework.Model.University.DTO
{
    public record UniversityDto(Guid Id,
                                string Name,
                                string Country,
                                string Webpage,
                                bool IsBookmarked);
}