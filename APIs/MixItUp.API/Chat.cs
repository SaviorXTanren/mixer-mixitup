using MixItUp.API.Models;
using System.Threading.Tasks;

namespace MixItUp.API
{
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

        public static async Task SendMessage(string message, bool sendAsStreamer)
        {
            var model = new SendChatMessage
            {
                Message = message,
                SendAsStreamer = sendAsStreamer
            };

            await RestClient.PostAsync("chat/message", model);
        }
    }
}
