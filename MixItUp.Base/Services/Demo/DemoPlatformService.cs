using MixItUp.Base.Model;
using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using StreamingClient.Base.Model.OAuth;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twitch.Base.Models.NewAPI.Users;

namespace MixItUp.Base.Services.Demo
{
    public class DemoPlatformService : StreamingPlatformServiceBase
    {
        public const string DemoFolder = "C:\\Mix It Up Demo";

        public static async Task<SettingsV3Model> CreateDemoSettings()
        {
            SettingsV3Model settings = new SettingsV3Model("Demo");
            settings.ID = new Guid("00000000-0000-0000-0000-000000000001");

            await ServiceManager.Get<IDatabaseService>().Write(settings.DatabaseFilePath, "DELETE FROM Commands");
            await ServiceManager.Get<IDatabaseService>().Write(settings.DatabaseFilePath, "DELETE FROM Quotes");
            await ServiceManager.Get<IDatabaseService>().Write(settings.DatabaseFilePath, "DELETE FROM Users");

#pragma warning disable CS0612 // Type or member is obsolete
            settings.DefaultStreamingPlatform = StreamingPlatformTypeEnum.Demo;
            settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Demo] = new StreamingPlatformAuthenticationSettingsModel()
            {
                Type = StreamingPlatformTypeEnum.Demo,
                UserID = ServiceManager.Get<DemoSessionService>().UserID,
                BotID = ServiceManager.Get<DemoSessionService>().BotID,
                ChannelID = ServiceManager.Get<DemoSessionService>().ChannelID,
                UserOAuthToken = new OAuthTokenModel(),
                BotOAuthToken = new OAuthTokenModel(),
            };
#pragma warning restore CS0612 // Type or member is obsolete

            settings.EnableOverlay = true;
            settings.OBSStudioServerIP = "ws://127.0.0.1:4455";
            settings.Counters["hello"] = new CounterModel("hello")
            {
                Amount = 0,
                ResetOnLoad = true
            };

            List<CommandModelBase> commands = new List<CommandModelBase>();

            commands.Add(GenerateChatCommand("Social Media", "socials", new List<ActionModelBase>()
            {
                new ChatActionModel("Be sure to follow me on Twitter, Instagram, and TikTok: @JoeSmoe")
            }));

            commands.Add(GenerateChatCommand("Shoutout", "so", new List<ActionModelBase>()
            {
                new ChatActionModel("Go check out @$username, they're an awesome streamer!"),
                new OverlayActionModel(null, new OverlayImageItemModel("C:\\Mix It Up Demo\\$useravatar", 400, 400)
                {
                    Position = new OverlayItemPositionModel(OverlayItemPositionType.Percentage, 50, 40, 0),
                    Effects = new OverlayItemEffectsModel(OverlayItemEffectEntranceAnimationTypeEnum.ZoomIn, OverlayItemEffectVisibleAnimationTypeEnum.Swing, OverlayItemEffectExitAnimationTypeEnum.ZoomOut, 5)
                }),
                new OverlayActionModel(null, new OverlayTextItemModel("$username", "White", 96, "Times New Roman", true, false, true, null)
                {
                    Position = new OverlayItemPositionModel(OverlayItemPositionType.Percentage, 50, 75, 0),
                    Effects = new OverlayItemEffectsModel(OverlayItemEffectEntranceAnimationTypeEnum.ZoomIn, OverlayItemEffectVisibleAnimationTypeEnum.Swing, OverlayItemEffectExitAnimationTypeEnum.ZoomOut, 5)
                })
            }));

            commands.Add(GenerateChatCommand("Be Right Back", "brb", new List<ActionModelBase>()
            {
                new ChatActionModel("Everyone stay put, we'll be right back!"),
                new StreamingSoftwareActionModel(StreamingSoftwareTypeEnum.OBSStudio, StreamingSoftwareActionTypeEnum.Scene)
                {
                    ItemName = "Be Right Back"
                },
                new WaitActionModel("10"),
                new StreamingSoftwareActionModel(StreamingSoftwareTypeEnum.OBSStudio, StreamingSoftwareActionTypeEnum.Scene)
                {
                    ItemName = "Main"
                },
                new ChatActionModel("And we're back!")
            }));

            commands.Add(GenerateChatCommand("Hello", "hi", new List<ActionModelBase>()
            {
                new CounterActionModel("hello", CounterActionTypeEnum.Update, "1"),
                new ChatActionModel("@$username said hi to @$targetusername! There have been $hello hello's this stream"),
            }));

            commands.Add(GenerateEventCommand(EventTypeEnum.ChannelFollowed, new List<ActionModelBase>()
            {
                new ChatActionModel("Thanks for the follow @$username!"),
                new SoundActionModel("C:\\Mix It Up Demo\\Assets\\Mega Man 8 - Stage Clear.mp3", 100)
            }));

            commands.Add(GenerateEventCommand(EventTypeEnum.ChannelRaided, new List<ActionModelBase>()
            {
                new ChatActionModel("@$username just raided us with $raidviewercount viewers!!!"),
                new OverlayActionModel(null, new OverlayTextItemModel("Thanks for the raid $username!", "White", 96, "Times New Roman", true, false, true, null)
                {
                    Position = new OverlayItemPositionModel(OverlayItemPositionType.Percentage, 50, 50, 0),
                    Effects = new OverlayItemEffectsModel(OverlayItemEffectEntranceAnimationTypeEnum.SlideInLeft, OverlayItemEffectVisibleAnimationTypeEnum.Bounce, OverlayItemEffectExitAnimationTypeEnum.SlideOutRight, 5)
                }),
                new SoundActionModel("C:\\Mix It Up Demo\\Assets\\Rick & Morty - My Man.mp3", 100)
            }));

            commands.Add(GenerateEventCommand(EventTypeEnum.ChannelSubscribed, new List<ActionModelBase>()
            {
                new ChatActionModel("@$username just $usersubtier subbed for the channel!"),
                new OverlayActionModel(null, new OverlayImageItemModel("C:\\Mix It Up Demo\\Assets\\Mega Man Clap.gif", 400, 400)
                {
                    Position = new OverlayItemPositionModel(OverlayItemPositionType.Percentage, 50, 50, 0),
                    Effects = new OverlayItemEffectsModel(OverlayItemEffectEntranceAnimationTypeEnum.ZoomIn, OverlayItemEffectVisibleAnimationTypeEnum.Swing, OverlayItemEffectExitAnimationTypeEnum.ZoomOut, 5)
                }),
                new SoundActionModel("C:\\Mix It Up Demo\\Assets\\Boom Goes The Dynamite.mp3", 100)
            }));
            
            commands.Add(GenerateEventCommand(EventTypeEnum.ChannelSubscriptionGifted, new List<ActionModelBase>()
            {
                new ChatActionModel("@$username just gifted out $usersubmonthsgifted subs to the channel!"),
                new SoundActionModel("C:\\Mix It Up Demo\\Assets\\Unfriggenbelievable.mp3", 100)
            }));

            await ServiceManager.Get<IDatabaseService>().BulkWrite(settings.DatabaseFilePath, "REPLACE INTO Commands(ID, TypeID, Data) VALUES($ID, $TypeID, $Data)",
                commands.Select(c => new Dictionary<string, object>() { { "$ID", c.ID.ToString() }, { "$TypeID", (int)c.Type }, { "$Data", JSONSerializerHelper.SerializeToString(c) } }));

            return settings;
        }

        public static Task<Result<DemoPlatformService>> Connect(OAuthTokenModel token)
        {
            return Task.FromResult(new Result<DemoPlatformService>(new DemoPlatformService()));
        }

        public static async Task<Result<DemoPlatformService>> ConnectUser()
        {
            return await DemoPlatformService.Connect();
        }

        public static async Task<Result<DemoPlatformService>> ConnectBot()
        {
            return await DemoPlatformService.Connect();
        }

        public static Task<Result<DemoPlatformService>> Connect()
        {
            return Task.FromResult(new Result<DemoPlatformService>(new DemoPlatformService()));
        }

        private static CommandModelBase GenerateChatCommand(string name, string trigger, List<ActionModelBase> actions)
        {
            ChatCommandModel command = new ChatCommandModel(name, new HashSet<string>() { trigger });
            command.Actions = actions;
            command.Requirements = new RequirementsSetModel(new List<RequirementModelBase>()
            {
                new RoleRequirementModel(StreamingPlatformTypeEnum.All, UserRoleEnum.User),
                new CooldownRequirementModel(CooldownTypeEnum.Standard, 5)
            });
            return command;
        }

        private static CommandModelBase GenerateEventCommand(EventTypeEnum eventType, List<ActionModelBase> actions)
        {
            EventCommandModel command = new EventCommandModel(eventType);
            command.Actions = actions;
            return command;
        }

        public override string Name { get { return MixItUp.Base.Resources.DemoConnection; } }

        public DemoPlatformService() { }

        public Task<UserModel> GetCurrentUser() { return Task.FromResult(DemoSessionService.user); }

        public Task<UserModel> GetUserByID(string userID)
        {
            UserModel user = null;
            if (string.Equals(DemoSessionService.user.id, userID))
            {
                user = DemoSessionService.user;
            }
            else if (string.Equals(DemoSessionService.bot.id, userID))
            {
                user = DemoSessionService.bot;
            }
            else
            {
                user = new UserModel()
                {
                    id = "user" + userID,
                    login = "user" + userID,
                    display_name = "User" + userID,
                    broadcaster_type = "",
                    profile_image_url = "https://raw.githubusercontent.com/SaviorXTanren/mixer-mixitup/master/Branding/MixItUp-Logo-Base-WhiteSM.png",
                };
            }
            return Task.FromResult(user);
        }

        public Task<UserModel> GetUserByLogin(string login)
        {
            UserModel user = null;
            if (string.Equals(DemoSessionService.user.login, login))
            {
                user = DemoSessionService.user;
            }
            else if (string.Equals(DemoSessionService.bot.login, login))
            {
                user = DemoSessionService.bot;
            }
            else
            {
                user = new UserModel()
                {
                    id = login,
                    login = login,
                    display_name = login,
                    broadcaster_type = "",
                    profile_image_url = "https://raw.githubusercontent.com/SaviorXTanren/mixer-mixitup/master/Branding/MixItUp-Logo-Base-WhiteSM.png",
                };
            }
            return Task.FromResult(user);
        }
    }
}
