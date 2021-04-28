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
    public class CommunityCommandsService
    {
        public const string UserAuthHeader = "MIUAuth";

        public async Task AddTestCommand()
        {
            StoreCommandUploadModel upload = new StoreCommandUploadModel()
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
                upload.Tags.Add("Sound");
            }
            else
            {
                upload.Tags.Add("Video");
            }

            ChatCommandModel command = new ChatCommandModel("Test", new HashSet<string>() { "test" });
            command.Actions.Add(new ChatActionModel("Hello World!"));
            upload.Command = command;

            await this.AddCommand(upload);
        }

        public async Task AddCommand(StoreCommandUploadModel command)
        {
            using (AdvancedHttpClient client = new AdvancedHttpClient())
            {
                client.DefaultRequestHeaders.Add(UserAuthHeader, ServiceManager.Get<SecretsService>().Encrypt(ChannelSession.Settings.ID.ToString()));
                HttpResponseMessage response = await client.PostAsync("https://localhost:44309/api/community/commands", AdvancedHttpClient.CreateContentFromObject(command));
            }
        }
    }
}
