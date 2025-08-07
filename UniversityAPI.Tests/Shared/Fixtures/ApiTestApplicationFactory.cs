namespace UniversityAPI.Tests.Shared.Fixtures
{
    public class ApiTestApplicationFactory : BaseTestApplicationFactory
    {
        protected override string DatabaseNamePrefix => "ApiTestDb";
    }
}