using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Requirements
{
    public class SettingsRequirementModel : RequirementModelBase
    {
        private static DateTimeOffset requirementErrorCooldown = DateTimeOffset.MinValue;

        [DataMember]
        public bool DeleteChatMessageWhenRun { get; set; }
        [DataMember]
        public bool DontDeleteChatMessageWhenRun { get; set; }

        [DataMember]
        public bool ShowOnChatContextMenu { get; set; }

        public SettingsRequirementModel() { }

        protected override DateTimeOffset RequirementErrorCooldown { get { return SettingsRequirementModel.requirementErrorCooldown; } set { SettingsRequirementModel.requirementErrorCooldown = value; } }

        public bool ShouldChatMessageBeDeletedWhenRun { get { return this.DeleteChatMessageWhenRun || (ChannelSession.Settings.DeleteChatCommandsWhenRun && !this.DontDeleteChatMessageWhenRun); } }
    }
}
