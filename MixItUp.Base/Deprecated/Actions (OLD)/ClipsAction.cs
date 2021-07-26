using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Twitch.Base.Models.NewAPI.Clips;

namespace MixItUp.Base.Actions
{
    [Obsolete]
    [DataContract]
    public class ClipsAction : ActionBase
    {
        public const string ClipURLSpecialIdentifier = "clipurl";

        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return ClipsAction.asyncSemaphore; } }

        [DataMember]
        public bool IncludeDelay { get; set; }

        [DataMember]
        public bool ShowClipInfoInChat { get; set; }

        public ClipsAction() : base(ActionTypeEnum.Clips) { }

        public ClipsAction(bool includeDelay, bool showClipInfoInChat)
            : this()
        {
            this.IncludeDelay = includeDelay;
            this.ShowClipInfoInChat = showClipInfoInChat;
        }

        protected override Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            return Task.CompletedTask;
        }
    }
}
