using AutoMapper;
using UniversityAPI.DataModel;

namespace UniversityAPI
{
    public class ProgramProfile :Profile
    {
        public ProgramProfile()
        {
            CreateMap<UniversityDM, UniversityDto>();
        }
    }
}
