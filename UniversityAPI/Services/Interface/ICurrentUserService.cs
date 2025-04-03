namespace UniversityAPI.Services
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }
        string UserName { get; }
    }
}
