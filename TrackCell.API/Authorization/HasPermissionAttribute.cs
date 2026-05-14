using Microsoft.AspNetCore.Authorization;

namespace TrackCell.API.Authorization
{
    public sealed class HasPermissionAttribute : AuthorizeAttribute
    {
        public HasPermissionAttribute(Resource resource, PermAction action)
        {
            Policy = Permissions.Policy(resource, action);
        }
    }
}
