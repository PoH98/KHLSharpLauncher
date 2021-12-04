using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace KHLBotSharp.WebHook.Helper
{
    public static class Helper
    {
        public static async Task<string> GetJson(this Stream stream)
        {
            using var ms = new MemoryStream(81920);
            await stream.CopyToAsync(ms);
            var bytes = ms.ToArray();
            var json = Encoding.UTF8.GetString(bytes);
            return json;
        }
    }
}