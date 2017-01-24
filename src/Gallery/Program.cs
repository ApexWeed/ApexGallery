using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace Gallery
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>();

            if (!string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development"))
            {
                // Need to set the listen url, however if in development setting the url will crash the server.
                builder.UseUrls("http://localhost:8002");
            }

            var host = builder.Build();
            host.Run();
        }
    }
}
