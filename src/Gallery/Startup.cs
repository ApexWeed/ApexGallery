using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Gallery
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            var apiSettings = Configuration.GetSection("Gallery:API");
            services.Configure<ApiSettings>(apiSettings);

            var interfaceSettings = Configuration.GetSection("Gallery:Interface");
            services.Configure<InterfaceSettings>(interfaceSettings);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes
                    .MapRoute(
                        "images",
                        "API/{lang}/Images/{path=}/{start=0}/{count=50}",
                        new { controller = "API", action= "Images" }
                    ).MapRoute(
                        "dirs",
                        "API/{lang}/Directory/{path=}",
                        new { controller = "API", action = "Directory" }
                    ).MapRoute(
                        "thumb-dir",
                        "API/ThumbDirectory/{path=}/{recurse=false}",
                        new { controller = "API", action = "ThumbDirectory" }
                    ).MapRoute(
                        "thumb-image",
                        "API/ThumbImages/{path=}/{start=0}/{count=50}",
                        new { controller = "API", action = "ThumbImages" }
                    ).MapRoute(
                        "api",
                        "API/{action=Index}/{path?}",
                        new { controller = "API" }
                    ).MapRoute(
                        "gallery",
                        "{lang?}",
                        new { controller = "Home", action = "Index" }
                    ).MapRoute(
                        "gallery-direct",
                        "Home/{lang?}",
                        new { controller = "Home", action = "Index" }
                    ).MapRoute(
                        "default",
                        "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
