namespace UniversityAPI.Framework.Model.User.DTO
{
    using System.ComponentModel.DataAnnotations;

    public record LoginDto
    {
        public LoginDto(string username,
                           string password)
        {
            Username = username;
            Password = password;
        }

        [Required(AllowEmptyStrings = false)]
        public string Username { get; init; }
        [Required(AllowEmptyStrings = false)]
        public string Password { get; init; }
    }
}