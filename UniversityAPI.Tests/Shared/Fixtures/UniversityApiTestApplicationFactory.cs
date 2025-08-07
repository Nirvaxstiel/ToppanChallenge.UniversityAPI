namespace UniversityAPI.Tests.Shared.Fixtures
{
    public class UniversityApiTestApplicationFactory : BaseTestApplicationFactory
    {
        protected override string DatabaseNamePrefix => "UniversityDomainDb";
    }
}