namespace UniversityAPI.Tests.Shared.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class TestAuthOptions
    {
        public string UserId { get; set; } = Guid.Empty.ToString();

        public string UserName { get; set; } = "TestUser";

        public string Role { get; set; } = "Admin";
    }
}
