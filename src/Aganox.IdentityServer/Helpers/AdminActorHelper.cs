using System.Security.Claims;
using Aganox.IdentityServer.Application.Models.Applications;

namespace Aganox.IdentityServer.Helpers;

public static class AdminActorHelper
{
    public static ActorContext FromHttpContext(HttpContext context) =>
        new(context.User.FindFirstValue(ClaimTypes.NameIdentifier));
}
