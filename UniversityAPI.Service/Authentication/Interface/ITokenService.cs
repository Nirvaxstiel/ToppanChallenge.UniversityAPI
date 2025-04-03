using UniversityAPI.Framework.Model;

namespace UniversityAPI.Service
{
    public interface ITokenService
    {
        Task<string> GenerateToken(UserDM user);
    }
}