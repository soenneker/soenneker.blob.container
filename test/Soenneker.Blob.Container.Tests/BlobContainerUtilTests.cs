using Soenneker.Blob.Container.Abstract;
using Soenneker.Tests.HostedUnit;

namespace Soenneker.Blob.Container.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public class BlobContainerUtilTests : HostedUnitTest
{
    private readonly IBlobContainerUtil _util;

    public BlobContainerUtilTests(Host host) : base(host)
    {
        _util = Resolve<IBlobContainerUtil>(true);
    }

    [Test]
    public void Default()
    {

    }
}
