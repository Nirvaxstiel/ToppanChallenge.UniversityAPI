namespace UniversityAPI.Framework.Model.User.DTO
{
    using System.ComponentModel.DataAnnotations;

    public record RegisterDto
    {
        public RegisterDto(string username,
                              string email,
                              string password)
        {
            Username = username;
            Email = email;
            Password = password;
        }

        [Required(AllowEmptyStrings = false)]
        public string Username { get; init; }
        [Required(AllowEmptyStrings = false)]
        [EmailAddress]
        public string Email { get; init; }
        [Required(AllowEmptyStrings = false)]
        public string Password { get; init; }
    }
}