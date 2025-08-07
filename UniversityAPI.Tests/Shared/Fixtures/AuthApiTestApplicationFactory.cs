namespace UniversityAPI.Tests.Shared.Fixtures
{
    public class AuthApiTestApplicationFactory : BaseTestApplicationFactory
    {
        protected override string DatabaseNamePrefix => "AuthDomainDb";
    }
}