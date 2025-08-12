namespace UniversityAPI.Service.Authentication.Interface
{
    using UniversityAPI.Framework.Model.User;

    public interface ITokenService
    {
        Task<string> GenerateToken(UserDM user);
    }
}