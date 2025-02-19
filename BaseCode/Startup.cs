using BaseCode.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Jose;

namespace BaseCode
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            var db_host = Environment.GetEnvironmentVariable("DB_HOST");
            var db_port = Environment.GetEnvironmentVariable("DB_PORT");
            var db_name = Environment.GetEnvironmentVariable("DB_NAME");
            var db_user = Environment.GetEnvironmentVariable("DB_USER");
            var db_password = Environment.GetEnvironmentVariable("DB_PASS");

            var conn = "Server=" + db_host + ";Port=" + db_port + ";Database=" + db_name + ";Uid=" + db_user + ";Pwd=" + db_password + ";Convert Zero Datetime=True";

            // Existing DBContext registration
            services.Add(new ServiceDescriptor(typeof(DBContext), new DBContext(conn)));
            services.Add(new ServiceDescriptor(typeof(DBCrudAct), new DBCrudAct(conn)));

            // Add DBCrudAct registration TEST
            // services.AddScoped<DBCrudAct>(provider => new DBCrudAct(conn));

            services.AddMvc().AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.PropertyNamingPolicy = null;
                o.JsonSerializerOptions.DictionaryKeyPolicy = null;

            });
            services.AddHttpContextAccessor();

            // Add JWT Authentication REVIEWHIN
            var jwtSettings = Configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"]
                };
            });

            // Add password reset JWT configuration
            services.Configure<JwtSettings>(jwtSettings);
            var resetKey = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]);
            Environment.SetEnvironmentVariable("JWT_SECRET_KEY", jwtSettings["SecretKey"]);
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
                //GetSettingResponse list = new GetSettingResponse();
                //var services = serviceScope.ServiceProvider;
                //var dbcon = services.GetService<DBContext>();
                //list = dbcon.GetSettingList("CORS");
                //foreach (Settings value in list.settings)
                //{

                //    }
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
