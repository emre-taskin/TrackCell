using Microsoft.AspNetCore.Authorization;

namespace TrackCell.API.Authorization
{
    public sealed class PermissionAuthorizationHandler
        : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            var expectedClaim = Permissions.PolicyToClaim(requirement.Permission);

            var granted = context.User.Claims.Any(c =>
                c.Type == Permissions.ClaimType &&
                c.Value == expectedClaim);

            if (granted)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
