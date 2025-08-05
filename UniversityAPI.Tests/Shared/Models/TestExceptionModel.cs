using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversityAPI.Framework.Model;

namespace UniversityAPI.Tests.Shared.Models
{
    public class TestExceptionModel
    {
       public  ApiException Exception { get; set; }
        public int ExpectedStatusCode { get; set; }
    }
}
