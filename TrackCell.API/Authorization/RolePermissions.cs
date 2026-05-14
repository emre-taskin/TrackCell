namespace TrackCell.API.Authorization
{
    public static class RolePermissions
    {
        public const string Admin = "Admin";
        public const string UserWriter = "UserWriter";
        public const string UserReader = "UserReader";

        public static IReadOnlyDictionary<string, HashSet<string>> Map { get; } = Build();

        private static Dictionary<string, HashSet<string>> Build()
        {
            var allRead = All(PermAction.Read);
            var allWrite = All(PermAction.Write);

            return new Dictionary<string, HashSet<string>>
            {
                [Admin] = allRead.Concat(allWrite).ToHashSet(),
                [UserWriter] = allRead.Concat(allWrite).ToHashSet(),
                [UserReader] = allRead.ToHashSet(),
            };
        }

        private static IEnumerable<string> All(PermAction action) =>
            Enum.GetValues<Resource>().Select(r => Permissions.Claim(r, action));
    }
}
