using KHLBotSharp.Common.MessageParser;
using KHLBotSharp.Core.Models.Config;
using KHLBotSharp.IService;
using KHLBotSharp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;

namespace KHLBotSharp.WebHook.NetCore3.Controllers
{
    public class HookController : Controller
    {
        private readonly IWebhookInstanceManagerService instanceManagerService;
        private readonly ILogService logService;
        public HookController(IWebhookInstanceManagerService webhookManager, ILogService logService)
        {
            instanceManagerService = webhookManager;
            this.logService = logService;
        }

        [Route("{**catchAll}")]
        public IActionResult Fuck()
        {
            logService.Warning(HttpContext.Connection.RemoteIpAddress.ToString() + " had accessed our bot illegally, lets send him to Tong Shen Serving Hot Pot");
            return RedirectPermanent("https://www.bilibili.com/video/BV1FZ4y1P7Wk/");
        }

        [HttpPost]
        [Route("/hook")]
        public async Task<IActionResult> Index(string botName)
        {
            try
            {
                var instance = instanceManagerService.Get(botName);
                var config = instance.ServiceProvider.GetRequiredService<IBotConfigSettings>();
                var pluginLoaderService = instance.ServiceProvider.GetRequiredService<IPluginLoaderService>();
                var logService = instance.ServiceProvider.GetRequiredService<ILogService>();
                var decoderService = instance.ServiceProvider.GetRequiredService<IDecoderService>();
                var memoryCache = instance.ServiceProvider.GetRequiredService<IMemoryCache>();
                var httpClient = instance.ServiceProvider.GetRequiredService<IHttpClientService>();
                if (!pluginLoaderService.Inited)
                {
                    pluginLoaderService.Init(instance.ServiceProvider);
                }
                if (HttpContext.Items["Content"] != null)
                {
                    string json = HttpContext.Items["Content"].ToString();
                    if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                    {
                        return new EmptyResult();
                    }
                    logService.Debug("Received call in webhook as url " + $"{ Request.Scheme }://{ Request.Host }{ Request.Path }{ Request.QueryString }\n{json}");
                    JToken jtoken = JToken.Parse(json);
                    JObject decoded;
                    try
                    {
                        decoded = await decoderService.DecodeEncrypt(jtoken);
                    }
                    catch (Exception ex)
                    {
                        logService.Error("Decrypt failed with " + ex.ToString());
                        return StatusCode(403);
                    }
                    var type = await decoderService.GetEventType(decoded);
                    //Check if token is correct
                    if (!decoded.Value<JObject>("d").ContainsKey("verify_token") || decoded.Value<JObject>("d").Value<string>("verify_token") != config.VerifyToken)
                    {
                        if (decoded.Value<JObject>("d").ContainsKey("verify_token"))
                        {
                            logService.Write("Received verify_token as " + decoded.Value<JObject>("d").Value<string>("verify_token"));
                        }
                        else
                        {
                            logService.Write("No verify_token found");
                        }
                        logService.Error("Invalid Token. Verification failed! Lets send him to Tong Shen Serving Hot Pot!");
                        return RedirectPermanent("https://www.bilibili.com/video/BV1FZ4y1P7Wk/");
                    }
                    switch (type)
                    {
                        case "Challenge":
                            var result = JObject.FromObject(new { challenge = decoded.Value<JObject>("d").Value<string>("challenge") }).ToString();
                            logService.Info("Challenge resolved successfully");
                            return Content(result, "application/json", Encoding.UTF8);
                        case "1":
                        case "2":
                        case "3":
                        case "9":
                        case "10":
                        case "255":
                            decoded.ParseEvent(pluginLoaderService, config, logService);
                            break;
                        default:
                            return StatusCode(403);
                    }
                    return StatusCode(200);
                }

            }
            catch (Exception ex)
            {
                logService.Error(ex.Message);
                logService.Write(HttpContext.Items["Content"].ToString());
            }
            return StatusCode(403);
        }
    }
}