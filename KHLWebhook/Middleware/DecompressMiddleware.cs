using KHLBotSharp.IService;
using KHLBotSharp.WebHook.Helper;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace KHLBotSharp.WebHook.Middleware
{
    //Copy from https://github.com/kaiheila-community/kaiheila-dotnet/blob/master/src/core/Client/WebHook/WebHookClient.cs
    public class DecompressMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CompressKey = "compress";
        public DecompressMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context, ILogService logService)
        {
            Stopwatch s = Stopwatch.StartNew();
            if (context.Request.Query.ContainsKey(CompressKey) && context.Request.Query[CompressKey] == "0")
            {
                context.Items["Content"] = await context.Request.Body.GetJson();
                await _next(context);
                s.Stop();
                logService.Write("Message processed in " + s.ElapsedMilliseconds + " ms");
                return;
            }

            // Decompress Deflate
            MemoryStream stream = new MemoryStream();
            await context.Request.Body.CopyToAsync(stream);
            await context.Request.Body.DisposeAsync();

            // Magic headers of zlib:
            // 78 01 - No Compression/low
            // 78 9C - Default Compression
            // 78 DA - Best Compression
            stream.Position = 2;

            DeflateStream deflateStream = new DeflateStream(stream, CompressionMode.Decompress);
            MemoryStream resultStream = new MemoryStream();
            await deflateStream.CopyToAsync(resultStream);
            await deflateStream.DisposeAsync();
            await stream.DisposeAsync();

            // Rewind
            resultStream.Position = 0;

            StreamReader reader = new StreamReader(resultStream);
            string result = await reader.ReadToEndAsync();
            await resultStream.DisposeAsync();

            context.Items["Content"] = result;
            reader.Dispose();
            await _next(context);
            await deflateStream.DisposeAsync();
            s.Stop();
            logService.Write("Message processed in " + s.ElapsedMilliseconds + " ms");
        }
    }
}