using KHLBotSharp.Core.BotHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace KHLBotSharp.WebHook.NetCore3
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Welcome.Print();
            await CreateHostBuilder(args).Build().RunBot();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            }).UseDefaultServiceProvider((context, options) =>
            {
                options.ValidateScopes = true;
                options.ValidateOnBuild = true;
            });
        }
    }
}