using MixItUp.API.Models;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace MixItUp.API
{
    //  GET:    http://localhost:8911/api/mixplay/users                                     MixPlay.GetUsersAsync
    //  GET:    http://localhost:8911/api/mixplay/user/{userID}                             MixPlay.GetUserByIDAsync
    //  GET:    http://localhost:8911/api/mixplay/user/search/username/{userName}           MixPlay.GetUserByUsernameAsync
    //  GET:    http://localhost:8911/api/mixplay/user/search/participant/{participantID}   MixPlay.GetUserByParticipantIDAsync
    //  POST:   http://localhost:8911/api/mixplay/broadcast                                 MixPlay.SendBroadcastAsync
    //  POST:   http://localhost:8911/api/mixplay/broadcast/users                           MixPlay.SendUsersBroadcastAsync

    public static class MixPlay
    {
        // User Methods
        public static async Task<MixPlayUser> GetUserByUsernameAsync(string userName)
        {
            return await RestClient.GetAsync<MixPlayUser>($"mixplay/user/search/username/{userName}");
        }

        public static async Task<MixPlayUser> GetUserByParticipantIDAsync(string participantID)
        {
            return await RestClient.GetAsync<MixPlayUser>($"mixplay/user/search/participant/{participantID}");
        }

        public static async Task<MixPlayUser> GetUserByIDAsync(uint userID)
        {
            return await RestClient.GetAsync<MixPlayUser>($"mixplay/user/{userID}");
        }

        public static async Task<MixPlayUser[]> GetUsersAsync()
        {
            return await RestClient.GetAsync<MixPlayUser[]>("mixplay/users");
        }
        
        // Broadcast Methods
        public static async Task SendUserBroadcastAsync(JObject data, MixPlayBroadcastUser user)
        {
            await SendUsersBroadcastAsync(data, new MixPlayBroadcastUser[] { user });
        }

        public static async Task SendGroupBroadcastAsync(JObject data, MixPlayBroadcastGroup group)
        {
            await SendBroadcastAsync(data, new MixPlayBroadcastTargetBase[] { group });
        }

        public static async Task SendParticipantBroadcastAsync(JObject data, MixPlayBroadcastParticipant participant)
        {
            await SendBroadcastAsync(data, new MixPlayBroadcastTargetBase[] { participant });
        }

        public static async Task SendSceneBroadcastAsync(JObject data, MixPlayBroadcastScene scene)
        {
            await SendBroadcastAsync(data, new MixPlayBroadcastTargetBase[] { scene });
        }

        public static async Task SendUsersBroadcastAsync(JObject data, MixPlayBroadcastUser[] users)
        {
            await RestClient.PostAsync($"mixplay/broadcast/users", new { data, users });
        }

        public static async Task SendGroupsBroadcastAsync(JObject data, MixPlayBroadcastGroup[] groups)
        {
            await SendBroadcastAsync(data, groups);
        }

        public static async Task SendParticipantsBroadcastAsync(JObject data, MixPlayBroadcastParticipant[] participants)
        {
            await SendBroadcastAsync(data, participants);
        }

        public static async Task SendScenesBroadcastAsync(JObject data, MixPlayBroadcastScene[] scenes)
        {
            await SendBroadcastAsync(data, scenes);
        }

        public static async Task SendBroadcastAsync(JObject data)
        {
            await SendBroadcastAsync(data, new MixPlayBroadcastTargetBase[] { new MixPlayBroadcastEveryone() });
        }

        public static async Task SendBroadcastAsync(JObject data, MixPlayBroadcastTargetBase[] targets)
        {
            await RestClient.PostAsync($"mixplay/broadcast", new { data, targets });
        }
    }
}