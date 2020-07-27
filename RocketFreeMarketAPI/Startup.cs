using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DataAccessLayer.Infrastructure;
using DataAccessLayer.DatabaseConnection;
using DataAccessLayer.Cryptography;
using DataAccessLayer.EmailSender;

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
                                                                builder.AllowAnyOrigin()
                                                                       .AllowAnyHeader()
                                                                       .AllowAnyMethod();
                                                            }
                                                            ));      

            services.AddControllers();
            services.AddSingleton<IAccountConnection, AccountConnection>();
            services.AddTransient<ICryptoProcess, CryptoProcess>();
            services.AddTransient<IEmailSender, EmailSender>();

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

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
