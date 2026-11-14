using Aganox.IdentityServer.Application.Validation;
using Aganox.IdentityServer.Domain.Enums;

namespace Aganox.IdentityServer.Tests.Validation;

public sealed class ApplicationStatusTransitionValidatorTests
{
    [Fact]
    public void Pending_to_Active_is_allowed()
    {
        ApplicationStatusTransitionValidator.Validate(ApplicationStatus.Pending, ApplicationStatus.Active);
    }

    [Fact]
    public void Revoked_to_Active_is_rejected()
    {
        Assert.Throws<ApplicationValidationException>(() =>
            ApplicationStatusTransitionValidator.Validate(ApplicationStatus.Revoked, ApplicationStatus.Active));
    }
}
