using MixItUp.API.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.API
{
    //  GET:    http://localhost:8911/api/mixplay/user/{participantID}      MixPlay.GetUserAsync
    //  GET:    http://localhost:8911/api/mixplay/users                     MixPlay.GetAllUsersAsync
    //  POST:   http://localhost:8911/api/mixplay/broadcast                 MixPlay.SendBroadcastAsync
    public static class MixPlay
    {
        public static async Task<MixPlayUser> GetUserAsync(string participantIDOrUserNameOrUserId)
        {
            return await RestClient.GetAsync<MixPlayUser>($"mixplay/user/{participantIDOrUserNameOrUserId}");
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

        public static async Task SendBroadcastToUserAsync(JObject data, string participantIDOrUserName)
        {
            var user = await GetUserAsync(participantIDOrUserName);
            if (user != null)
            {
                var model = new MixPlayBroadcast()
                {
                    Scopes = user.ParticipantIDs.Select(id => $"participant:{id}").ToList(),
                    Data = data,
                };
                await RestClient.PostAsync("mixplay/broadcast", model);
            }
        }

        public static Task SendBroadcastToUserAsync(JObject data, uint userID)
        {
            return SendBroadcastToUserAsync(data, userID.ToString());
        }
    }
}