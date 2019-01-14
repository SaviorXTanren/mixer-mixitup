using MixItUp.API.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.API
{
    //  GET:    http://localhost:8911/api/mixplay/user/{participantID}      MixPlay.GetUserAsync
    //  GET:    http://localhost:8911/api/mixplay/users                     MixPlay.GetAllUsersAsync
    //  POST:   http://localhost:8911/api/mixplay/broadcast                 MixPlay.SendBroadcastAsync
    public static class MixPlay
    {
        public static async Task<MixPlayUser> GetUserAsync(string participantID)
        {
            return await RestClient.GetAsync<MixPlayUser>($"mixplay/user/{participantID}");
        }

        public static async Task<MixPlayUser[]> GetAllUsersAsync()
        {
            return await RestClient.GetAsync<MixPlayUser[]>("mixplay/users");
        }

        public static async Task SendBroadcastAsync(JObject data, List<string> scopes)
        {
            var model = new MixPlayBroadcast()
            {
                Scopes = scopes,
                Data = data,
            };
            await RestClient.PostAsync("mixplay/broadcast", model);
        }
    }
}