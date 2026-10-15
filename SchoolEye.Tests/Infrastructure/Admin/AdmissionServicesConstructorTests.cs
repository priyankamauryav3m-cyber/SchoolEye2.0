using Infrastructure.Admin;
using Infrastructure.SuperAdmin;
using Microsoft.Extensions.Configuration;

namespace SchoolEye.Tests.Infrastructure.Admin;

/// <summary>
/// Services open SqlConnection internally, so only the configuration guard is unit-testable.
/// Stored-procedure behaviour requires integration tests against a dedicated test database.
/// </summary>
public class AdmissionServicesConstructorTests
{
    private const string Key = "DatabaseSettings1:ConnectionString";

    // Fake value only — no server, no credentials. Never opened by these tests.
    private const string FakeConnectionString = "Server=unit-test-placeholder;Database=none;Integrated Security=true";

    private static IConfiguration Config(string? connectionString)
    {
        var values = new Dictionary<string, string?>();
        if (connectionString != null)
            values[Key] = connectionString;
        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }

    public static TheoryData<string> Services => new() { nameof(EnquiryService), nameof(RegistrationService), nameof(AdmissionDateService) };

    private static object Create(string service, IConfiguration config) => service switch
    {
        nameof(EnquiryService) => new EnquiryService(config),
        nameof(RegistrationService) => new RegistrationService(config),
        nameof(AdmissionDateService) => new AdmissionDateService(config),
        _ => throw new ArgumentOutOfRangeException(nameof(service))
    };

    [Theory]
    [MemberData(nameof(Services))]
    public void Constructor_MissingConnectionString_ThrowsArgumentNullException(string service)
    {
        var ex = Assert.Throws<ArgumentNullException>(() => Create(service, Config(null)));

        Assert.Equal(Key, ex.ParamName);
    }

    [Theory]
    [MemberData(nameof(Services))]
    public void Constructor_ConnectionStringPresent_Constructs(string service)
    {
        Assert.NotNull(Create(service, Config(FakeConnectionString)));
    }
}
