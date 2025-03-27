using BaseCode.Models;
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

            // Log the connection strings for debugging purposes
            // Console.WriteLine($"Primary Connection String: {conn}");
            // Console.WriteLine($"Dealership Connection String: {dealershipConn}");

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

            // Register database contexts
            services.Add(new ServiceDescriptor(typeof(DBContext), new DBContext(conn)));
            services.Add(new ServiceDescriptor(typeof(DBCrudAct), new DBCrudAct(conn, Configuration)));
            services.Add(new ServiceDescriptor(typeof(DealershipDBContext), new DealershipDBContext(dealershipConn)));
            
            // CarService removed as its functionality is now in DealershipDBContext

            services.AddMvc().AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.PropertyNamingPolicy = null;
                o.JsonSerializerOptions.DictionaryKeyPolicy = null;
            });
            services.AddHttpContextAccessor();

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
}
