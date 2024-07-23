using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MixItUp.API
{
    internal static class RestClient
    {
        private const string BaseUri = "http://localhost:8911/api/";

        public static async Task<T> GetAsync<T>(string path)
        {
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(BaseUri)
            };

            HttpResponseMessage message = await client.GetAsync(path);
            if (message.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(await message.Content.ReadAsStringAsync());
            }

            return default(T);
        }

        public static async Task PostAsync(string path, object body)
        {
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(BaseUri)
            };

            var json = JsonConvert.SerializeObject(body);
            HttpContent content = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
            await client.PostAsync(path, content);
        }

        public static async Task<T> PostAsync<T>(string path, object body)
        {
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(BaseUri)
            };

            var json = JsonConvert.SerializeObject(body);
            HttpContent content = new StringContent(json, UnicodeEncoding.UTF8, "application/json");

            HttpResponseMessage message = await client.PostAsync(path, content);
            if (message.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(await message.Content.ReadAsStringAsync());
            }

            return default(T);
        }

        public static async Task<T> PutAsync<T>(string path, object body)
        {
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(BaseUri)
            };

            var json = JsonConvert.SerializeObject(body);
            HttpContent content = new StringContent(json, UnicodeEncoding.UTF8, "application/json");

            HttpResponseMessage message = await client.PutAsync(path, content);
            if (message.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(await message.Content.ReadAsStringAsync());
            }

            return default(T);
        }

        public static async Task DeleteAsync(string path)
        {
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(BaseUri)
            };

            await client.DeleteAsync(path);
        }
    }
}
