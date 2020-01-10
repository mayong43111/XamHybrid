using Newtonsoft.Json;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace XHApp.Actions
{
    public static class PPTSHttpClient
    {
        public static async Task<T> PostAsync<T>(string address, NameValueCollection data)
        {
            WebClient client = PrepareWebClient();

            string query = string.Join("&",
               data.AllKeys.Select(key => key + "=" + HttpUtility.UrlEncode(data[key])));

            string json = await client.UploadStringTaskAsync(address, "POST", query);

            return JsonConvert.DeserializeObject<T>(json);
        }

        public static async Task<T> PostByJsonAsync<T>(string address, object data)
        {
            WebClient client = PrepareWebClient();

            string json = await client.UploadStringTaskAsync(address, "POST", JsonConvert.SerializeObject(data));

            return JsonConvert.DeserializeObject<T>(json);
        }


        public static async Task<T> GetAsync<T>(string address)
        {
            WebClient client = PrepareWebClient();

            string json = await client.DownloadStringTaskAsync(address);

            return JsonConvert.DeserializeObject<T>(json);
        }

        private static WebClient PrepareWebClient()
        {
            WebClient client = new WebClient
            {
                Encoding = Encoding.UTF8
            };
            client.Headers["Content-Type"] = "application/json";

            return client;
        }
    }
}
