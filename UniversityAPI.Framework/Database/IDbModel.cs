using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversityAPI.Framework.Database
{
    public interface IDbModel
    {
        DateTime CreatedDate { get; set; }
        DateTime? UpdatedDate { get; set; }
        Guid CreatedBy { get; set; }
        Guid? UpdatedBy { get; set; }
        bool IsActive { get; set; }
    }
}
