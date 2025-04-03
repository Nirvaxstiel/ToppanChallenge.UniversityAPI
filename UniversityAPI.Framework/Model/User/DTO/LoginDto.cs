using System.ComponentModel.DataAnnotations;

namespace UniversityAPI.Framework.Model
{
    public class LoginDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
