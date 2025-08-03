using System.ComponentModel.DataAnnotations;

namespace UniversityAPI.Framework.Model
{
    public class RegisterDto
    {
        [Required(AllowEmptyStrings = false)]
        public string Username { get; set; }

        [Required(AllowEmptyStrings = false)]
        [EmailAddress]
        public string Email { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Password { get; set; }
    }
}