using System.Threading.Tasks;
using Azure.Storage.Blobs;
using FluentAssertions;
using Soenneker.Blob.Container.Abstract;
using Soenneker.Extensions.ValueTask;
using Soenneker.Facts.Local;
using Soenneker.Tests.FixturedUnit;
using Xunit;
using Xunit.Abstractions;

namespace Soenneker.Blob.Container.Tests;

[Collection("Collection")]
public class BlobContainerUtilTests : FixturedUnitTest
{
    private readonly IBlobContainerUtil _util;

    public BlobContainerUtilTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<IBlobContainerUtil>();
    }

    [LocalFact]
    public async Task Get_should_return_client()
    {
        BlobContainerClient client = await _util.Get("test").NoSync();

        client.Should().NotBeNull();
    }
}
