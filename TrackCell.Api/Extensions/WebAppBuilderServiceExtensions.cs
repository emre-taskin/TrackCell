namespace MES.API.Extensions
{
    public static class WebAppBuilderServiceExtensions
    {
        public static IServiceCollection ConfigureDatabaseContext(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString, x => x.MigrationsAssembly("MES.Infrastructure")));
            return services;
        }

        public static IServiceCollection ConfigureRedis(this IServiceCollection services, string connectionString)
        {
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(connectionString));
            return services;
        }

        public static IServiceCollection ConfigureSignalR(this IServiceCollection services)
        {
            services.AddSignalR();
            return services;
        }

        public static IServiceCollection ConfigureAuthentication(this IServiceCollection services)
        {
            services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();
            return services;
        }

        public static IServiceCollection ConfigureAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

                options.AddPolicy(Constants.Policy.Name.ProjectRead, policy => policy.RequireClaim(Constants.Claim.Type.Permission, Constants.Claim.Value.ProjectRead));
                options.AddPolicy(Constants.Policy.Name.ProjectWrite, policy => policy.RequireClaim(Constants.Claim.Type.Permission, Constants.Claim.Value.ProjectWrite));
                options.AddPolicy(Constants.Policy.Name.AuthorizationRead, policy => policy.RequireClaim(Constants.Claim.Type.Permission, Constants.Claim.Value.AuthorizationRead));
                options.AddPolicy(Constants.Policy.Name.AuthorizationWrite, policy => policy.RequireClaim(Constants.Claim.Type.Permission, Constants.Claim.Value.AuthorizationWrite));
            });
            return services;
        }

        public static IServiceCollection ConfigureMapper(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg => cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()));
            return services;
        }

        public static IServiceCollection ConfigureServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IUserRoleService, UserRoleService>();
            services.AddScoped<IMasterDataService, MasterDataService>();
            services.AddScoped<IOperationHistoryService, OperationHistoryService>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IClaimsTransformation, ClaimsTransformation>();
            return services;
        }
    }
}
