namespace UniversityAPI.Utility.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class LocalizationOptions
    {
        public string DefaultCulture { get; set; } = "en";
        public List<string> SupportedCultures { get; set; } = new();
    }
}
