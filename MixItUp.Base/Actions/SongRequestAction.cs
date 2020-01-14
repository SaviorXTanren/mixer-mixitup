using MixItUp.Base.Model.SongRequests;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
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
        PauseResumeCurrentSong,
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
        PauseCurrentSong,
        [Name("Resume Current Song")]
        ResumeCurrentSong,
        [Name("Ban Current Song")]
        BanCurrentSong
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
#pragma warning disable CS0612 // Type or member is obsolete
            : base(ActionTypeEnum.SongRequest)
#pragma warning restore CS0612 // Type or member is obsolete
        {
            this.SpecificService = SongRequestServiceTypeEnum.All;
        }

        public SongRequestAction(SongRequestActionTypeEnum songRequestType, SongRequestServiceTypeEnum service = SongRequestServiceTypeEnum.All)
            : this()
        {
            this.SongRequestType = songRequestType;
            this.SpecificService = service;
        }

        protected override Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            return Task.FromResult(0);
        }
    }
}
