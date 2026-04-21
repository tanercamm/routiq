namespace Routsky.Api.Services;

public interface IDecisionSolverService
{
    Task<DecisionSolverService.DecisionResult> SolveAsync(
        Guid groupId,
        Func<string, Task>? onStatus = null,
        CancellationToken ct = default);

    Task<DecisionSolverService.DecisionResult> SolveDiscoverAsync(
        DecisionSolverService.DiscoverRequest request,
        Func<string, Task>? onStatus = null,
        CancellationToken ct = default);
}
