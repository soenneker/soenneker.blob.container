using Soenneker.Blob.Container.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;


namespace Soenneker.Blob.Container.Tests;

[Collection("Collection")]
public class BlobContainerUtilTests : FixturedUnitTest
{
    private readonly IBlobContainerUtil _util;

    public BlobContainerUtilTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<IBlobContainerUtil>(true);
    }

    [Fact]
    public void Default()
    {

    }
}
