using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [Obsolete]
    public enum OvrStreamActionTypeEnum
    {
        [Obsolete]
        [Name("Show Title")]
        ShowTitle,
        [Name("Hide Title")]
        HideTitle,
        [Name("Play Title")]
        PlayTitle,
        [Name("Update Variables")]
        UpdateVariables,
        [Name("Enable Title")]
        EnableTitle,
        [Name("Disable Title")]
        DisableTitle,
    }

    [Obsolete]
    [DataContract]
    public class OvrStreamAction : ActionBase
    {
        public static OvrStreamAction CreateVariableTitleAction(OvrStreamActionTypeEnum actionType, string titleName, Dictionary<string, string> variables)
        {
            return new OvrStreamAction(actionType)
            {
                TitleName = titleName,
                Variables = variables,
            };
        }

        public static OvrStreamAction CreateTitleAction(OvrStreamActionTypeEnum actionType, string titleName)
        {
            return new OvrStreamAction(actionType)
            {
                TitleName = titleName,
            };
        }

        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return OvrStreamAction.asyncSemaphore; } }

        [DataMember]
        public OvrStreamActionTypeEnum OvrStreamActionType { get; set; }

        [DataMember]
        public string TitleName { get; set; }

        [DataMember]
        public Dictionary<string, string> Variables { get; set; }

        public OvrStreamAction()
            : base(ActionTypeEnum.OvrStream)
        {
        }

        public OvrStreamAction(OvrStreamActionTypeEnum ovrStreamActionType)
            : this()
        {
            this.OvrStreamActionType = ovrStreamActionType;
        }

        protected override Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            return Task.FromResult(0);
        }
    }
}
