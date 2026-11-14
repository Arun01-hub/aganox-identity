using Aganox.IdentityServer.Domain.Enums;

namespace Aganox.IdentityServer.Application.Models.Applications;

public sealed class ApplicationListQuery
{
    public ApplicationStatus? Status { get; init; }

    public DeploymentEnvironment? Environment { get; init; }

    public ApplicationType? ApplicationType { get; init; }

    public string? Search { get; init; }

    public bool IncludeArchived { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}
