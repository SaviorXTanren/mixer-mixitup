using Mixer.Base.Clients;
using Mixer.Base.Model.User;
using System;
using System.Threading.Tasks;

namespace MixItUp.Base.MixerAPI
{
    public class ChatClientWrapper : MixerRequestWrapperBase
    {
        public ChatClient Client { get; private set; }

        public ChatClientWrapper(ChatClient client)
        {
            this.Client = client;
        }

        public async Task<bool> ConnectAndAuthenticate() { return await this.RunAsync(this.Client.Connect()) && await this.RunAsync(this.Client.Authenticate()); }

        public async Task SendMessage(string message) { await this.RunAsync(this.Client.SendMessage(message)); }

        public async Task Whisper(string username, string message) { await this.RunAsync(this.Client.Whisper(username, message)); }

        public async Task DeleteMessage(Guid id) { await this.RunAsync(this.Client.DeleteMessage(id)); }

        public async Task ClearMessages() { await this.RunAsync(this.Client.ClearMessages()); }

        public async Task PurgeUser(string username) { await this.RunAsync(this.Client.PurgeUser(username)); }

        public async Task TimeoutUser(string username, uint durationInSeconds) { await this.RunAsync(this.Client.TimeoutUser(username, durationInSeconds)); }

        public async Task Disconnect() { await this.RunAsync(this.Client.Disconnect()); }
    }
}
