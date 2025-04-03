namespace UniversityAPI.Service
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }
        string UserName { get; }
    }
}
