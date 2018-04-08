using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data;
using AutoMapper;

namespace PluralsightWebAPI
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsEnvironment("Development"))
            {
                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }

            builder.AddEnvironmentVariables();
            _config = builder.Build();
        }

        IConfigurationRoot _config;

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(_config);
            services.AddDbContext<CampContext>(ServiceLifetime.Scoped);
            services.AddScoped<ICampRepository, CampRepository>();
            services.AddTransient<CampDbInitializer>();

            //Profile class under the models folder serves as the mapping Entity object
            //This tells Automapper how to map the objects
            services.AddAutoMapper();
            // Add framework services.
            //services.AddApplicationInsightsTelemetry(Configuration);
            //Control Serialization to avoid infinite loop for return child objects
            services.AddMvc()
                .AddJsonOptions(opt =>
                {
                    opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                })
                ;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            CampDbInitializer seeder)
        {
            loggerFactory.AddConsole(_config.GetSection("Logging"));
            loggerFactory.AddDebug();

           // app.UseApplicationInsightsRequestTelemetry();

           // app.UseApplicationInsightsExceptionTelemetry();

            app.UseMvc();
            //Initialized DB
            seeder.Seed().Wait();
        }
    }
}
