using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Twitch.Base.Models.NewAPI.Clips;

namespace MixItUp.Base.Model.Actions
{
    [DataContract]
    public class ClipsActionModel : ActionModelBase
    {
        public const string ClipURLSpecialIdentifier = "clipurl";

        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return ClipsActionModel.asyncSemaphore; } }

        [DataMember]
        public bool IncludeDelay { get; set; }

        [DataMember]
        public bool ShowClipInfoInChat { get; set; }

        public ClipsActionModel(bool includeDelay, bool showClipInfoInChat)
            : base(ActionTypeEnum.Clips)
        {
            this.IncludeDelay = includeDelay;
            this.ShowClipInfoInChat = showClipInfoInChat;
        }

        protected override async Task PerformInternal(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            ClipCreationModel clipCreation = await ChannelSession.TwitchUserConnection.CreateClip(ChannelSession.TwitchUserNewAPI, this.IncludeDelay);
            if (clipCreation != null)
            {
                for (int i = 0; i < 12; i++)
                {
                    await Task.Delay(5000);

                    ClipModel clip = await ChannelSession.TwitchUserConnection.GetClip(clipCreation);
                    if (clip != null && !string.IsNullOrEmpty(clip.url))
                    {
                        if (this.ShowClipInfoInChat)
                        {
                            await ChannelSession.Services.Chat.SendMessage("Clip Created: " + clip.url);
                        }
                        specialIdentifiers[ClipURLSpecialIdentifier] = clip.url;

                        GlobalEvents.TwitchClipCreated(clip);
                        return;
                    }
                }
            }
            await ChannelSession.Services.Chat.SendMessage(MixItUp.Base.Resources.ClipCreationFailed);
        }
    }
}
