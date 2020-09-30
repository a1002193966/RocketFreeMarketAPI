using System.Text;
using DataAccessLayer.Cryptography;
using DataAccessLayer.DatabaseConnection;
using DataAccessLayer.EmailSender;
using DataAccessLayer.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace RocketFreeMarketAPI
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
          
            services.AddCors(options => options.AddPolicy(name: "CorsPolicy",
                                                            builder => {
                                                                builder
                                                                .WithOrigins("http://localhost:4200", "https://localhost:44358")
                                                                .AllowAnyMethod()
                                                                .AllowAnyHeader();
                                                                //.AllowCredentials();
                                                            }));                                                           
            services.AddControllers();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration.GetSection("JWT").GetSection("Issuer").Value,
                    ValidAudience = Configuration.GetSection("JWT").GetSection("Issuer").Value,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetSection("JWT").GetSection("Key").Value))
                };
            });
            services.AddTransient<IAccountConnection, AccountConnection>();
            services.AddTransient<IUserConnection, UserConnection>();
            services.AddTransient<IProductPostConnection, ProductPostConnection>();
            services.AddTransient<ICryptoProcess, CryptoProcess>();
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<ILoginToken, LoginToken>();
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
            app.UseCors("CorsPolicy");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
