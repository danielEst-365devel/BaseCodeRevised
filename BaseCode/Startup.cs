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
            var db_host = Environment.GetEnvironmentVariable("DB_HOST");
            var db_port = Environment.GetEnvironmentVariable("DB_PORT");
            var db_name = Environment.GetEnvironmentVariable("DB_NAME");
            var db_user = Environment.GetEnvironmentVariable("DB_USER");
            var db_password = Environment.GetEnvironmentVariable("DB_PASS");

            var conn = $"Server={db_host};Port={db_port};Database={db_name};Uid={db_user};Pwd={db_password};Convert Zero Datetime=True";

            // Log the connection string for debugging purposes
            Console.WriteLine($"Connection String: {conn}");

            // Build a new configuration that includes the connection string under "DefaultConnection"
            var connectionStringConfig = new Dictionary<string, string>
                {
                    { "ConnectionStrings:DefaultConnection", conn }
                };

            var configBuilder = new ConfigurationBuilder()
                .AddConfiguration(Configuration)
                .AddInMemoryCollection(connectionStringConfig);
            Configuration = configBuilder.Build();

            // Existing DBContext registration
            services.Add(new ServiceDescriptor(typeof(DBContext), new DBContext(conn)));
            // Update the DBCrudAct service registration to include the IConfiguration parameter
            services.Add(new ServiceDescriptor(typeof(DBCrudAct), new DBCrudAct(conn, Configuration)));
            

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
