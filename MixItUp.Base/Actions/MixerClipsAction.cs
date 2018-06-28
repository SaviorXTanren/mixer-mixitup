using Mixer.Base.Model.Broadcast;
using Mixer.Base.Model.Clips;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class MixerClipsAction : ActionBase
    {
        public const int MinimumLength = 15;
        public const int MaximumLength = 300;

        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return MixerClipsAction.asyncSemaphore; } }

        [DataMember]
        public string ClipName { get; set; }

        [DataMember]
        public int ClipLength { get; set; }

        public MixerClipsAction() : base(ActionTypeEnum.MixerClips) { }

        public MixerClipsAction(string clipName, int clipLength)
            : this()
        {
            this.ClipName = clipName;
            this.ClipLength = clipLength;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.Chat != null)
            {
                await ChannelSession.Chat.SendMessage("Sending clip creation request to Mixer...");

                string clipName = await this.ReplaceStringWithSpecialModifiers(this.ClipName, user, arguments);
                if (!string.IsNullOrEmpty(clipName) && MixerClipsAction.MinimumLength <= this.ClipLength && this.ClipLength <= MixerClipsAction.MaximumLength)
                {
                    bool clipCreated = false;
                    DateTimeOffset clipCreationTime = DateTimeOffset.Now;

                    BroadcastModel broadcast = await ChannelSession.Connection.GetCurrentBroadcast();
                    if (broadcast != null)
                    {
                        if (await ChannelSession.Connection.CanClipBeMade(broadcast))
                        {
                            clipCreated = await ChannelSession.Connection.CreateClip(new ClipRequestModel()
                            {
                                broadcastId = broadcast.id.ToString(),
                                highlightTitle = clipName,
                                clipDurationInSeconds = this.ClipLength
                            });
                        }
                    }

                    if (clipCreated)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            await Task.Delay(2000);

                            IEnumerable<ClipModel> clips = await ChannelSession.Connection.GetChannelClips(ChannelSession.Channel);
                            ClipModel clip = clips.OrderByDescending(c => c.uploadDate).FirstOrDefault();
                            if (clip != null && clip.uploadDate.ToLocalTime() >= clipCreationTime && clip.title.Equals(clipName))
                            {
                                await ChannelSession.Chat.SendMessage("Clip Created: " + string.Format("https://mixer.com/{0}?clip={1}", ChannelSession.User.username, clip.shareableId));
                                return;
                            }
                        }
                        await ChannelSession.Chat.SendMessage("Clip was created, but could not be retrieved at this time");
                    }
                    else
                    {
                        await ChannelSession.Chat.SendMessage("Unable to create clip, please try again later");
                    }
                }
            }
        }
    }
}
