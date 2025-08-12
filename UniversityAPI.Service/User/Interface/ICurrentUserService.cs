namespace UniversityAPI.Service.User.Interface
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }
        string UserName { get; }
    }
}