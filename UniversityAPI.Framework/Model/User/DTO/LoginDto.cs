using System.ComponentModel.DataAnnotations;

namespace UniversityAPI.Framework.Model
{
    public class LoginDto
    {
        [Required(AllowEmptyStrings = false)]
        public string Username { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Password { get; set; }
    }
}