namespace TrackCell.API.Authorization
{
    public static class Permissions
    {
        public const string Prefix = "Perm";
        public const string ClaimType = "permission";

        public static string Policy(Resource resource, PermAction action) =>
            $"{Prefix}.{resource}.{action}";

        public static string Claim(Resource resource, PermAction action) =>
            $"{resource}:{action}";

        public static string PolicyToClaim(string policy)
        {
            var parts = policy.Split('.');
            return parts.Length == 3 && parts[0] == Prefix
                ? $"{parts[1]}:{parts[2]}"
                : policy;
        }
    }
}
