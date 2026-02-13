using Routiq.Api.DTOs;

namespace Routiq.Api.Services;

public interface IRouteGenerator
{
    Task<RouteResponseDto> GenerateRoutesAsync(RouteRequestDto request);
}
