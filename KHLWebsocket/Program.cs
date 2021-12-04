using KHLBotSharp.Core.BotHost;
using System;
using System.IO;
using System.Threading.Tasks;

namespace KHLSharp.Websocket
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Welcome.Print();
            if (args.Length > 1)
            {
                var profileName = args[1];
                if (args[0] == "-c")
                {
                    if (!Directory.Exists("Profiles"))
                    {
                        Directory.CreateDirectory("Profiles");
                    }
                    if (!Directory.Exists("Profiles\\" + profileName))
                    {
                        Directory.CreateDirectory("Profiles\\" + profileName);
                        Directory.CreateDirectory("Profiles\\" + profileName + "\\Plugins");
                    }
                    Console.WriteLine("Profile creation success!");
                    return;
                }
                else if (args[0] == "-r")
                {
                    if (!Directory.Exists("Profiles"))
                    {
                        Directory.CreateDirectory("Profiles");
                    }
                    if (!Directory.Exists("Profiles\\" + profileName))
                    {
                        Directory.CreateDirectory("Profiles\\" + profileName);
                        Directory.CreateDirectory("Profiles\\" + profileName + "\\Plugins");
                    }
                    var bot = "Profiles\\" + profileName;
                    var botService = new BotService(bot);
                    _ = botService.Run();
                }
            }
            else
            {
                if (!Directory.Exists("Profiles"))
                {
                    Directory.CreateDirectory("Profiles");
                    Directory.CreateDirectory("Profiles\\DefaultBot");
                    Directory.CreateDirectory("Profiles\\DefaultBot\\Plugins");
                }
                var bots = Directory.GetDirectories("Profiles");
                foreach (var bot in bots)
                {
                    var botService = new BotService(bot);
                    _ = botService.Run();
                }
            }
            await Task.Delay(-1);
        }
    }
}
