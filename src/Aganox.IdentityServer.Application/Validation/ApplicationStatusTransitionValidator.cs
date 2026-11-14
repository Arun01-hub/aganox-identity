using Aganox.IdentityServer.Domain.Enums;

namespace Aganox.IdentityServer.Application.Validation;

public static class ApplicationStatusTransitionValidator
{
    private static readonly Dictionary<ApplicationStatus, HashSet<ApplicationStatus>> Allowed = new()
    {
        [ApplicationStatus.Pending] = [ApplicationStatus.Active, ApplicationStatus.Archived],
        [ApplicationStatus.Active] = [ApplicationStatus.Disabled, ApplicationStatus.Revoked, ApplicationStatus.Archived],
        [ApplicationStatus.Disabled] = [ApplicationStatus.Active, ApplicationStatus.Revoked, ApplicationStatus.Archived],
        [ApplicationStatus.Revoked] = [ApplicationStatus.Archived],
        [ApplicationStatus.Archived] = []
    };

    public static void Validate(ApplicationStatus current, ApplicationStatus target)
    {
        if (current == target)
        {
            return;
        }

        if (!Allowed.TryGetValue(current, out var targets) || !targets.Contains(target))
        {
            throw new ApplicationValidationException(
                $"Cannot transition application status from {current} to {target}.");
        }
    }
}
