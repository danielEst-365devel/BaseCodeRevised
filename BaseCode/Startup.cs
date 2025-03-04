using BaseCode.Models;
using Jose;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

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

            string connectionString;
            bool useOnlineDb = bool.Parse(Environment.GetEnvironmentVariable("USE_ONLINE_DB") ?? "false");

            if (useOnlineDb)
            {
                // Use online Azure SQL Database
                var server = Environment.GetEnvironmentVariable("ONLINE_DB_SERVER");
                var dbName = Environment.GetEnvironmentVariable("ONLINE_DB_NAME");
                var dbUser = Environment.GetEnvironmentVariable("ONLINE_DB_USER");
                var dbPass = Environment.GetEnvironmentVariable("ONLINE_DB_PASS");

                connectionString = $"Server={server};Initial Catalog={dbName};Persist Security Info=False;User ID={dbUser};Password={dbPass};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

                Console.WriteLine("Using ONLINE database connection");
            }
            else
            {
                // Use local MySQL database
                var db_host = Environment.GetEnvironmentVariable("DB_HOST");
                var db_port = Environment.GetEnvironmentVariable("DB_PORT");
                var db_name = Environment.GetEnvironmentVariable("DB_NAME");
                var db_user = Environment.GetEnvironmentVariable("DB_USER");
                var db_password = Environment.GetEnvironmentVariable("DB_PASS");

                connectionString = $"Server={db_host};Port={db_port};Database={db_name};Uid={db_user};Pwd={db_password};Convert Zero Datetime=True";

                Console.WriteLine("Using LOCAL database connection");
            }

            // Log the connection string for debugging purposes (mask password in production)
            Console.WriteLine($"Connection String: {MaskPassword(connectionString)}");

            // Build a new configuration that includes the connection string under "DefaultConnection"
            var connectionStringConfig = new Dictionary<string, string>
    {
        { "ConnectionStrings:DefaultConnection", connectionString }
    };

            var configBuilder = new ConfigurationBuilder()
                .AddConfiguration(Configuration)
                .AddInMemoryCollection(connectionStringConfig);
            Configuration = configBuilder.Build();

            // Register services with the connection string
            services.Add(new ServiceDescriptor(typeof(DBContext), new DBContext(connectionString)));
            services.Add(new ServiceDescriptor(typeof(DBCrudAct), new DBCrudAct(connectionString, Configuration)));



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

        // Helper method to mask password in logs
        private string MaskPassword(string connectionString)
        {
            // Simple regex-free approach to mask password
            if (connectionString.Contains("Password="))
            {
                int startIndex = connectionString.IndexOf("Password=") + 9;
                int endIndex = connectionString.IndexOf(';', startIndex);
                if (endIndex == -1) endIndex = connectionString.Length;

                return connectionString.Substring(0, startIndex) + "********" +
                       (endIndex < connectionString.Length ? connectionString.Substring(endIndex) : string.Empty);
            }

            if (connectionString.Contains("Pwd="))
            {
                int startIndex = connectionString.IndexOf("Pwd=") + 4;
                int endIndex = connectionString.IndexOf(';', startIndex);
                if (endIndex == -1) endIndex = connectionString.Length;

                return connectionString.Substring(0, startIndex) + "********" +
                       (endIndex < connectionString.Length ? connectionString.Substring(endIndex) : string.Empty);
            }

            return connectionString;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            //app.UseAuthentication();

            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                // Example scope usage; uncomment if needed
                // var services = serviceScope.ServiceProvider;
                // var dbcon = services.GetService<DBContext>();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication(); // Add this line before UseAuthorization
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
