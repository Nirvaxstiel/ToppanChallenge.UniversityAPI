using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversityAPI.Utility
{
    public interface IAppConfigAccessor
    {
        string GetValue(string key);
        IConfigurationSection GetSection(string key);
    }
}
