using MixItUp.API.V1.Models;
using System.Threading.Tasks;

namespace MixItUp.API
{
    //  GET:    http://localhost:8911/api/chat/users        Chat.GetAllUsersAsync
    //  DELETE: http://localhost:8911/api/chat/message      Chat.ClearChatAsync
    //  POST:   http://localhost:8911/api/chat/message      Chat.SendMessageAsync
    public static class Chat
    {
        public static async Task<User[]> GetAllUsersAsync()
        {
            return await RestClient.GetAsync<User[]>("chat/users");
        }

        public static async Task ClearChatAsync()
        {
            await RestClient.DeleteAsync("chat/message");
        }

        public static async Task SendMessageAsync(string message, bool sendAsStreamer)
        {
            var model = new SendChatMessage
            {
                Message = message,
                SendAsStreamer = sendAsStreamer
            };

            await RestClient.PostAsync("chat/message", model);
        }

        public static async Task SendWhisperAsync(string userName, string message, bool sendAsStreamer)
        {
            var model = new SendChatWhisper
            {
                Message = message,
                UserName = userName,
                SendAsStreamer = sendAsStreamer
            };

            await RestClient.PostAsync("chat/whisper", model);
        }
    }
}
