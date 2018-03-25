using Mixer.Base.Util;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public enum SongRequestActionTypeEnum
    {
        [Name("Add Song To Queue")]
        AddSongToQueue,
        [Name("Display Currently Playing")]
        DisplayCurrentlyPlaying,
        [Name("Display Next Song")]
        DisplayNextSong,
    }

    public class SongRequestAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return SongRequestAction.asyncSemaphore; } }

        [DataMember]
        public SongRequestActionTypeEnum SongRequestType { get; set; }

        public SongRequestAction() : base(ActionTypeEnum.SongRequest) { }

        public SongRequestAction(SongRequestActionTypeEnum songRequestType)
            : this()
        {
            this.SongRequestType = songRequestType;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.Chat != null)
            {
                if (this.SongRequestType == SongRequestActionTypeEnum.AddSongToQueue)
                {
                    await ChannelSession.Services.SongRequestService.AddSongRequest(user, string.Join(" ", arguments));
                }
                else if (this.SongRequestType == SongRequestActionTypeEnum.DisplayCurrentlyPlaying)
                {
                    SongRequestItem currentlyPlaying = await ChannelSession.Services.SongRequestService.GetCurrentlyPlaying();
                    if (currentlyPlaying != null)
                    {
                        await ChannelSession.Chat.SendMessage("Currently Playing: " + currentlyPlaying.Name);
                    }
                    else
                    {
                        await ChannelSession.Chat.SendMessage("There is currently no song playing for the Song Request queue");
                    }
                }
                else if (this.SongRequestType == SongRequestActionTypeEnum.DisplayNextSong)
                {
                    SongRequestItem nextTrack = await ChannelSession.Services.SongRequestService.GetNextTrack();
                    if (nextTrack != null)
                    {
                        await ChannelSession.Chat.SendMessage("Coming Up Next: " + nextTrack.Name);
                    }
                    else
                    {
                        await ChannelSession.Chat.SendMessage("There are currently no Song Requests left in the queue");
                    }
                }
            }
        }
    }
}
