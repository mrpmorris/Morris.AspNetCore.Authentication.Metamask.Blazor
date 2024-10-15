using Microsoft.AspNetCore.Authentication;

namespace Morris.AspNetCore.Authentication.Metamask.Blazor;

internal record ChallengeContent(Uri Host, DateTimeOffset ExpiresAfter, string Account)
{
    public bool HasExpired(ISystemClock clock) => ExpiresAfter <= clock.UtcNow;
}
