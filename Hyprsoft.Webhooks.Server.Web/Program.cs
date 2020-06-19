using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Hyprsoft.Webhooks.Server.Web.Hangfire;

namespace Hyprsoft.Webhooks.Server.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var hostBuilder = Host.CreateDefaultBuilder(args);
            if (args.Length > 1 && args[0].Contains("service"))
                hostBuilder.UseWindowsService();
            hostBuilder.ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());

            return hostBuilder;
        }
    }
}
