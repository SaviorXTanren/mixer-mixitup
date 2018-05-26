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
        [Name("Play/Pause Current Song")]
        PlayPauseCurrentSong,
        [Name("Skip To Next Song")]
        SkipToNextSong,
        [Name("Enable/Disable Song Requests")]
        EnableDisableSongRequests,
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
                if (this.SongRequestType == SongRequestActionTypeEnum.EnableDisableSongRequests)
                {
                    if (!ChannelSession.Services.SongRequestService.IsEnabled)
                    {
                        if (!await ChannelSession.Services.SongRequestService.Initialize())
                        {
                            ChannelSession.Services.SongRequestService.Disable();
                            await ChannelSession.Chat.Whisper(user.UserName, "Song Requests were not able to enabled, please try manually enabling it.");
                            return;
                        }
                    }
                    else
                    {
                        ChannelSession.Services.SongRequestService.Disable();
                    }
                }
                else
                {
                    if (ChannelSession.Services.SongRequestService == null || !ChannelSession.Services.SongRequestService.IsEnabled)
                    {
                        await ChannelSession.Chat.Whisper(user.UserName, "Song Requests are not currently enabled");
                        return;
                    }

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
                    else if (this.SongRequestType == SongRequestActionTypeEnum.PlayPauseCurrentSong)
                    {
                        await ChannelSession.Services.SongRequestService.PlayPauseCurrentSong();
                    }
                    else if (this.SongRequestType == SongRequestActionTypeEnum.SkipToNextSong)
                    {
                        await ChannelSession.Services.SongRequestService.SkipToNextSong();
                    }
                }
            }
        }
    }
}
