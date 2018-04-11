using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace Memstate.Host.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment environment)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(environment.ContentRootPath)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();

            Console.WriteLine("Resources:");

            foreach (var resource in GetType().GetTypeInfo().Assembly.GetManifestResourceNames())
            {
                Console.WriteLine(resource);
            }
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder application, IHostingEnvironment environment, ILoggerFactory logger)
        {
            logger.AddConsole(Configuration.GetSection("Logging"));
            logger.AddDebug();

            if (environment.IsDevelopment())
            {
                application.UseDeveloperExceptionPage();
            }
            else
            {
                application.UseExceptionHandler("/error");
            }

            application.UseDefaultFiles(
                new DefaultFilesOptions
                {
                    FileProvider = new EmbeddedFileProvider(GetType().GetTypeInfo().Assembly, GetType().Namespace + ".wwwroot")
                });

            application.UseStaticFiles(
                new StaticFileOptions
                {
                    FileProvider = new EmbeddedFileProvider(GetType().GetTypeInfo().Assembly, GetType().Namespace + ".wwwroot")
                });

            application.UseMvc();
        }
    }
}