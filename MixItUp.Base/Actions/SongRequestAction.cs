using Mixer.Base.Util;
using MixItUp.Base.Model.SongRequests;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public enum SongRequestActionTypeEnum
    {
        [Name("Search Songs & Manually Select Result")]
        SearchSongsAndSelectResult,
        [Obsolete]
        [Name("Display Currently Playing")]
        DisplayCurrentlyPlaying,
        [Obsolete]
        [Name("Display Next Song")]
        DisplayNextSong,
        [Name("Pause/Resume Current Song")]
        PauseResume,
        [Name("Skip To Next Song")]
        SkipToNextSong,
        [Name("Enable/Disable Song Requests")]
        EnableDisableSongRequests,
        [Name("Set Volume")]
        SetVolume,
        [Name("Remove Last Song Requested By User")]
        RemoveLastByUser,
        [Name("Search Songs & Pick First Result")]
        SearchSongsAndPickFirstResult,
        [Name("Remove Last Song Requested")]
        RemoveLast,
        [Name("Pause Current Song")]
        Pause,
        [Name("Resume Current Song")]
        Resume,
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
                        if (!await ChannelSession.Services.SongRequestService.Enable())
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

                    if (this.SongRequestType == SongRequestActionTypeEnum.SearchSongsAndSelectResult ||
                        this.SongRequestType == SongRequestActionTypeEnum.SearchSongsAndPickFirstResult)
                    {
                        if (ChannelSession.Settings.SongRequestsMaxRequests > 0)
                        {
                            IEnumerable<SongRequestModel> requestedSongs = ChannelSession.Services.SongRequestService.RequestSongs.ToList().Where(s => s.User.Equals(user));
                            if (requestedSongs.Count() >= ChannelSession.Settings.SongRequestsMaxRequests)
                            {
                                await ChannelSession.Chat.Whisper(user.UserName, string.Format("You already have {0} song requests active, which is the max amount allowed", ChannelSession.Settings.SongRequestsMaxRequests));
                                return;
                            }
                        }

                        if (this.SongRequestType == SongRequestActionTypeEnum.SearchSongsAndSelectResult)
                        {
                            await ChannelSession.Services.SongRequestService.SearchAndSelect(user, this.SpecificService, string.Join(" ", arguments));
                        }
                        else if (this.SongRequestType == SongRequestActionTypeEnum.SearchSongsAndPickFirstResult)
                        {
                            await ChannelSession.Services.SongRequestService.SearchAndPickFirst(user, this.SpecificService, string.Join(" ", arguments));
                        }
                    }
                    else if (this.SongRequestType == SongRequestActionTypeEnum.RemoveLast)
                    {
                        await ChannelSession.Services.SongRequestService.RemoveLastRequested();
                    }
                    else if (this.SongRequestType == SongRequestActionTypeEnum.RemoveLastByUser)
                    {
                        await ChannelSession.Services.SongRequestService.RemoveLastRequested(user);
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
                                return;
                            }
                        }
                        await ChannelSession.Chat.Whisper(user.UserName, "Please specify a volume level [0-100].");
                        return;
                    }
#pragma warning disable CS0612 // Type or member is obsolete
                    else if (this.SongRequestType == SongRequestActionTypeEnum.DisplayCurrentlyPlaying)
#pragma warning restore CS0612 // Type or member is obsolete
                    {
                        SongRequestModel currentlyPlaying = await ChannelSession.Services.SongRequestService.GetCurrent();
                        if (currentlyPlaying != null)
                        {
                            await ChannelSession.Chat.SendMessage("Currently Playing: " + currentlyPlaying.Name);
                        }
                        else
                        {
                            await ChannelSession.Chat.SendMessage("There is currently no song playing for the Song Request queue");
                        }
                    }
#pragma warning disable CS0612 // Type or member is obsolete
                    else if (this.SongRequestType == SongRequestActionTypeEnum.DisplayNextSong)
#pragma warning restore CS0612 // Type or member is obsolete
                    {
                        SongRequestModel nextTrack = await ChannelSession.Services.SongRequestService.GetNext();
                        if (nextTrack != null)
                        {
                            await ChannelSession.Chat.SendMessage("Coming Up Next: " + nextTrack.Name);
                        }
                        else
                        {
                            await ChannelSession.Chat.SendMessage("There are currently no Song Requests left in the queue");
                        }
                    }
                    else if (this.SongRequestType == SongRequestActionTypeEnum.Pause)
                    {
                        await ChannelSession.Services.SongRequestService.Pause();
                    }
                    else if (this.SongRequestType == SongRequestActionTypeEnum.Resume)
                    {
                        await ChannelSession.Services.SongRequestService.Resume();
                    }
                    else if (this.SongRequestType == SongRequestActionTypeEnum.PauseResume)
                    {
                        await ChannelSession.Services.SongRequestService.PauseResume();
                    }
                    else if (this.SongRequestType == SongRequestActionTypeEnum.SkipToNextSong)
                    {
                        await ChannelSession.Services.SongRequestService.Skip();
                    }
                }
            }
        }
    }
}
