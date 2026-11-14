using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Aganox.IdentityServer.Tests.Integration;

[Collection("IdentityIntegration")]
public sealed class AdminApplicationsApiTests(IdentityWebApplicationFactory factory)
{
    [Fact]
    public async Task Admin_applications_requires_authentication()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/api/admin/applications");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
