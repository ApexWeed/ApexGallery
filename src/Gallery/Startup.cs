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
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            var APISettings = Configuration.GetSection("Gallery:API");
            services.Configure<APISettings>(APISettings);

            var InterfaceSettings = Configuration.GetSection("Gallery:Interface");
            services.Configure<InterfaceSettings>(InterfaceSettings);
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
                        name: "images",
                        template: "API/{lang}/Images/{path=}/{start=0}/{count=50}",
                        defaults: new { controller = "API", action= "Images" }
                    ).MapRoute(
                        name: "dirs",
                        template: "API/{lang}/Directory/{path=}",
                        defaults: new { controller = "API", action = "Directory" }
                    ).MapRoute(
                        name: "thumb-dir",
                        template: "API/ThumbDirectory/{path=}/{recurse=false}",
                        defaults: new { controller = "API", action = "ThumbDirectory" }
                    ).MapRoute(
                        name: "thumb-image",
                        template: "API/ThumbImages/{path=}/{start=0}/{count=50}",
                        defaults: new { controller = "API", action = "ThumbImages" }
                    ).MapRoute(
                        name: "api",
                        template: "API/{action=Index}/{path?}",
                        defaults: new { controller = "API" }
                    ).MapRoute(
                        name: "gallery",
                        template: "{lang?}",
                        defaults: new { controller = "Home", action = "Index" }
                    ).MapRoute(
                        name: "gallery-direct",
                        template: "Home/{lang?}",
                        defaults: new { controller = "Home", action = "Index" }
                    ).MapRoute(
                        name: "default",
                        template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
