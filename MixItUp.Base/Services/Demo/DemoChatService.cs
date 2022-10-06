using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Demo
{
    public class DemoChatService : StreamingPlatformServiceBase
    {
        private SemaphoreSlim messageSemaphore = new SemaphoreSlim(1);

        private List<ChatMessageViewModel> messages = new List<ChatMessageViewModel>();

        private List<string> precannedMessages = new List<string>()
        {
            "Yo, how's the stream going?",
            "Hey!",
            "!socials",
            "HAH, he got rekt",
            "Loving the stream, I'll catch you later!",
            "lol",
            "Hey, can I join next match?",
            "!hi @viewer3",
            "Wow, this is boring...",
            "New here, wanted to say hi?",
            "Whatcha playing?",
            ":)",
        };

        public override string Name { get { return "Mock Chat"; } }

        public Task<Result> ConnectUser()
        {
            AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
            {
                while (true)
                {
                    for (int i = 0; i < precannedMessages.Count; i++)
                    {
                        await Task.Delay(15000);

                        await this.GenerateAndAddMessage("Viewer" + (i + 1), precannedMessages[i]);

                        if (i % 2 == 1)
                        {
                            await Task.Delay(15000);

                            CommandParametersModel parameters = new CommandParametersModel();
#pragma warning disable CS0612 // Type or member is obsolete
                            parameters.Platform = StreamingPlatformTypeEnum.Demo;
                            parameters.User = await ServiceManager.Get<UserService>().GetUserByPlatformUsername(StreamingPlatformTypeEnum.Demo,
                                "Viewer" + (i + 1),
                                performPlatformSearch: true);
                            parameters.TargetUser = parameters.User = await ServiceManager.Get<UserService>().GetUserByPlatformUsername(StreamingPlatformTypeEnum.Demo,
                                "Viewer" + i,
                                performPlatformSearch: true);
#pragma warning restore CS0612 // Type or member is obsolete
                            parameters.Arguments = new List<string>() { "Hello World!" };
                            parameters.SpecialIdentifiers["raidviewercount"] = "123";
                            parameters.SpecialIdentifiers["message"] = "Test Message";
                            parameters.SpecialIdentifiers["usersubplanname"] = "Plan Name";
                            parameters.SpecialIdentifiers["usersubplan"] = "Tier 1";
                            parameters.SpecialIdentifiers["usersubmonthsgifted"] = "3";
                            parameters.SpecialIdentifiers["isanonymous"] = "false";

                            if (i == 1 || i == 5)
                            {
                                await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.ChannelFollowed, parameters);
                            }
                            else if (i == 3)
                            {
                                await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.ChannelRaided, parameters);
                            }
                            else if (i == 7)
                            {
                                await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.ChannelSubscribed, parameters);
                            }
                            else if (i == 9)
                            {
                                await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.ChannelSubscriptionGifted, parameters);
                            }
                        }
                    }
                }               

            }, new CancellationToken());

            return Task.FromResult(new Result());
        }

        public Task<Result> ConnectBot() { return Task.FromResult(new Result()); }

        public Task DisconnectUser() { return Task.CompletedTask; }

        public Task DisconnectBot() { return Task.CompletedTask; }

        public async Task SendMessage(string message, bool sendAsStreamer = false, string replyMessageID = null)
        {
            await this.messageSemaphore.WaitAndRelease(async () =>
            {
                await this.GenerateAndAddMessage((sendAsStreamer) ? ServiceManager.Get<DemoSessionService>().Username : ServiceManager.Get<DemoSessionService>().Botname, message);
            });
        }

        public Task<bool> DeleteMessage(ChatMessageViewModel message)
        {
            this.messages.Remove(message);

            return Task.FromResult(true);
        }

        private async Task GenerateAndAddMessage(string username, string message)
        {
#pragma warning disable CS0612 // Type or member is obsolete
            UserV2ViewModel user = await ServiceManager.Get<UserService>().GetUserByPlatformUsername(StreamingPlatformTypeEnum.Demo,
                username,
                performPlatformSearch: true);

            UserChatMessageViewModel messageViewModel = new UserChatMessageViewModel(Guid.NewGuid().ToString(), StreamingPlatformTypeEnum.Demo, user);
#pragma warning restore CS0612 // Type or member is obsolete

            foreach (string part in message.Split(new char[] { ' ' }))
            {
                messageViewModel.AddStringMessagePart(part);
            }

            this.messages.Add(messageViewModel);

            await ServiceManager.Get<ChatService>().AddMessage(messageViewModel);
        }
    }
}
