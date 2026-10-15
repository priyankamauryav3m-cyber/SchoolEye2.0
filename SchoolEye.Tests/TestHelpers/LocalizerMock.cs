using DomainModel.Resources.Resource;
using Microsoft.Extensions.Localization;
using Moq;

namespace SchoolEye.Tests.TestHelpers;

/// <summary>
/// IStringLocalizer&lt;Resource&gt; that returns the resource key itself,
/// so tests can assert which key a controller used.
/// </summary>
public static class LocalizerMock
{
    public static Mock<IStringLocalizer<Resource>> Create()
    {
        var mock = new Mock<IStringLocalizer<Resource>>();
        mock.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));
        mock.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string key, object[] _) => new LocalizedString(key, key));
        return mock;
    }
}
