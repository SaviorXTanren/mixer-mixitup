using Mixer.Base.Util;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public enum SongRequestActionTypeEnum
    {
        [Name("Search Songs & Use Artist Lookup")]
        SearchSongsAndUseArtistSelect,
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
        [Name("Set Song Request Volume")]
        SetVolume,
        [Name("Remove Last Song Requested By User")]
        RemoveLastByUser,
        [Name("Search Songs & Pick First Result")]
        SearchSongsAndPickFirstResult,
        [Name("Remove Last Song Requested")]
        RemoveLast,
    }

    public class SongRequestAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return SongRequestAction.asyncSemaphore; } }

        [DataMember]
        public SongRequestActionTypeEnum SongRequestType { get; set; }

        [DataMember]
        public SongRequestServiceTypeEnum SpecificService { get; set; }

        public SongRequestAction()
            : base(ActionTypeEnum.SongRequest)
        {
            this.SpecificService = SongRequestServiceTypeEnum.All;
        }

        public SongRequestAction(SongRequestActionTypeEnum songRequestType, SongRequestServiceTypeEnum service = SongRequestServiceTypeEnum.All)
            : this()
        {
            this.SongRequestType = songRequestType;
            this.SpecificService = service;
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
                            await ChannelSession.Services.SongRequestService.Disable();
                            await ChannelSession.Chat.Whisper(user.UserName, "Song Requests were not able to enabled, please try manually enabling it.");
                            return;
                        }
                    }
                    else
                    {
                        await ChannelSession.Services.SongRequestService.Disable();
                    }
                }
                else
                {
                    if (ChannelSession.Services.SongRequestService == null || !ChannelSession.Services.SongRequestService.IsEnabled)
                    {
                        await ChannelSession.Chat.Whisper(user.UserName, "Song Requests are not currently enabled");
                        return;
                    }

                    if (this.SongRequestType == SongRequestActionTypeEnum.SearchSongsAndUseArtistSelect)
                    {
                        await ChannelSession.Services.SongRequestService.AddSongRequest(user, this.SpecificService, string.Join(" ", arguments));
                    }
                    else if (this.SongRequestType == SongRequestActionTypeEnum.SearchSongsAndPickFirstResult)
                    {
                        await ChannelSession.Services.SongRequestService.AddSongRequest(user, this.SpecificService, string.Join(" ", arguments), pickFirst: true);
                    }
                    else if (this.SongRequestType == SongRequestActionTypeEnum.RemoveLast)
                    {
                        await ChannelSession.Services.SongRequestService.RemoveLastSongRequested();
                    }
                    else if (this.SongRequestType == SongRequestActionTypeEnum.RemoveLastByUser)
                    {
                        await ChannelSession.Services.SongRequestService.RemoveLastSongRequestedByUser(user);
                    }
                    else if (this.SongRequestType == SongRequestActionTypeEnum.SetVolume)
                    {
                        if (arguments != null && arguments.Count() > 0)
                        {
                            string volumeAmount = await this.ReplaceStringWithSpecialModifiers(arguments.First(), user, arguments);
                            if (int.TryParse(volumeAmount, out int volume))
                            {
                                volume = MathHelper.Clamp(volume, 0, 100);
                                ChannelSession.Settings.SongRequestVolume = volume;
                                await ChannelSession.Services.SongRequestService.RefreshVolume();
                                await ChannelSession.Chat.SendMessage("Song request volume set to " + ChannelSession.Settings.SongRequestVolume);
                                return;
                            }
                        }
                        await ChannelSession.Chat.Whisper(user.UserName, "Please specify a volume level [0-100].");
                        return;
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
