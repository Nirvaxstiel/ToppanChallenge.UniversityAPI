using UniversityAPI.DataModel;

namespace UniversityAPI.Services
{
    public interface ITokenService
    {
        Task<string> GenerateToken(UserDM user);
    }
}
