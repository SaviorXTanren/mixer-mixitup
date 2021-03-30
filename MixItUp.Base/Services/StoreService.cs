using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Store;
using MixItUp.Base.Util;
using StreamingClient.Base.Web;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public class StoreService
    {
        public const string UserAuthHeader = "MIUAuth";

        public async Task AddTestCommand()
        {
            StoreCommandUploadModel command = new StoreCommandUploadModel()
            {
                ID = Guid.NewGuid(),
                Name = "Test Command " + RandomHelper.GenerateRandomNumber(100),
                Description = "Here's some description text",
                Tags = new HashSet<string>() { "Chat" },
                MixItUpUserID = ChannelSession.Settings.ID.ToString(),
                Username = ChannelSession.GetCurrentUser().Username,
                UserAvatarURL = ChannelSession.GetCurrentUser().AvatarLink,
            };

            if (RandomHelper.GenerateRandomNumber(100) > 50)
            {
                command.Tags.Add("Sound");
            }
            else
            {
                command.Tags.Add("Video");
            }

            ChatCommandModel c = new ChatCommandModel("Test", new HashSet<string>() { "test" });
            c.Actions.Add(new ChatActionModel("Hello World!"));

            command.Commands.Add(c);

            await this.AddCommand(command);
        }

        public async Task AddCommand(StoreCommandUploadModel command)
        {
            using (AdvancedHttpClient client = new AdvancedHttpClient())
            {
                client.DefaultRequestHeaders.Add(UserAuthHeader, ChannelSession.Services.Secrets.Encrypt(ChannelSession.Settings.ID.ToString()));
                HttpResponseMessage response = await client.PostAsync("https://localhost:44309/api/store/command", AdvancedHttpClient.CreateContentFromObject(command));
            }
        }
    }
}
