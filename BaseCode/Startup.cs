using BaseCode.Models;
using BaseCode.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace BaseCode
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Primary database connection (BASE)
            var db_host = Environment.GetEnvironmentVariable("DB_HOST");
            var db_port = Environment.GetEnvironmentVariable("DB_PORT");
            var db_name = Environment.GetEnvironmentVariable("DB_NAME");
            var db_user = Environment.GetEnvironmentVariable("DB_USER");
            var db_password = Environment.GetEnvironmentVariable("DB_PASS");

            var conn = $"Server={db_host};Port={db_port};Database={db_name};Uid={db_user};Pwd={db_password};Convert Zero Datetime=True";

            // Secondary database connection (DEALERSHIP)
            var dealership_host = Environment.GetEnvironmentVariable("DEALERSHIP_DB_HOST");
            var dealership_port = Environment.GetEnvironmentVariable("DEALERSHIP_DB_PORT");
            var dealership_name = Environment.GetEnvironmentVariable("DEALERSHIP_DB_NAME");
            var dealership_user = Environment.GetEnvironmentVariable("DEALERSHIP_DB_USER");
            var dealership_password = Environment.GetEnvironmentVariable("DEALERSHIP_DB_PASS");

            var dealershipConn = $"Server={dealership_host};Port={dealership_port};Database={dealership_name};Uid={dealership_user};Pwd={dealership_password};Convert Zero Datetime=True";

            // Build a new configuration that includes both connection strings
            var connectionStringConfig = new Dictionary<string, string>
            {
                { "ConnectionStrings:DefaultConnection", conn },
                { "ConnectionStrings:DealershipConnection", dealershipConn }
            };

            var configBuilder = new ConfigurationBuilder()
                .AddConfiguration(Configuration)
                .AddInMemoryCollection(connectionStringConfig);
            Configuration = configBuilder.Build();

            // Add HTTP context accessor first so it can be used by ApiLogService
            services.AddHttpContextAccessor();

            // Register two separate ApiLogService instances for BASE and DEALERSHIP databases
            // 1. Create a BASE database ApiLogService
            services.AddSingleton<BaseApiLogService>(provider => new BaseApiLogService(
                conn,
                provider.GetRequiredService<Microsoft.AspNetCore.Http.IHttpContextAccessor>()));

            // 2. Create a DEALERSHIP database ApiLogService
            services.AddSingleton<DealershipApiLogService>(provider => new DealershipApiLogService(
                dealershipConn,
                provider.GetRequiredService<Microsoft.AspNetCore.Http.IHttpContextAccessor>()));

            // Register database contexts
            services.Add(new ServiceDescriptor(typeof(DBContext), new DBContext(conn)));

            // Update DBCrudAct registration to include BASE ApiLogService
            services.Add(new ServiceDescriptor(
                typeof(DBCrudAct),
                provider => new DBCrudAct(
                    conn,
                    Configuration,
                    provider.GetRequiredService<BaseApiLogService>()
                ),
                ServiceLifetime.Singleton));

            // Register DealershipDBContext with DEALERSHIP ApiLogService
            services.AddSingleton(provider =>
                new DealershipDBContext(
                    dealershipConn,
                    provider.GetRequiredService<DealershipApiLogService>()));

            services.AddMvc().AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.PropertyNamingPolicy = null;
                o.JsonSerializerOptions.DictionaryKeyPolicy = null;
            });

            // Add session-based authentication with JWT
            services.AddAuthentication("SessionAuth")
                .AddScheme<AuthenticationSchemeOptions, SessionAuthenticationHandler>("SessionAuth", null);

            // Add authorization for RBAC
            services.AddAuthorization(options =>
            {
                options.AddPolicy("CanCreateUsers", policy =>
                    policy.RequireClaim("permission", "CreateUser"));

                options.AddPolicy("CanViewActiveUsers", policy =>
                    policy.RequireClaim("permission", "ViewActiveUsers"));

                options.AddPolicy("CanUpdateUserDetails", policy =>
                    policy.RequireClaim("permission", "UpdateUserDetails"));
            });

            services.AddHostedService<SessionExpirationService>(); // Register background service

            services.AddSingleton<IConfiguration>(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

    // Create specialized ApiLogService classes to differentiate between BASE and DEALERSHIP
    public class BaseApiLogService : ApiLogService
    {
        public BaseApiLogService(string connectionString, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(connectionString, httpContextAccessor) { }
    }

    public class DealershipApiLogService : ApiLogService
    {
        public DealershipApiLogService(string connectionString, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(connectionString, httpContextAccessor) { }
    }
}
