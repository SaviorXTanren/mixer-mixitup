using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum ActionTypeEnum
    {
        Custom = 0,
        [Name("ChatMessage")]
        Chat,
        [Name("ConsumablesCurrencyRankEtc")]
        Consumables,
        ExternalProgram,
        [Name("InputKeyboardAndMouse")]
        Input,
        [Name("OverlayImagesAndVideos")]
        Overlay,
        Sound,
        Wait,
        [Name("CounterCreateAndUpdate")]
        Counter,
        GameQueue,
        TextToSpeech,
        WebRequest,
        SpecialIdentifier,
        [Name("FileReadAndWrite")]
        File,
        Discord,
        [Obsolete]
        Translation,
        Twitter,
        Conditional,
        [Name("StreamingSoftwareOBSSLOBS")]
        StreamingSoftware,
        Streamlabs,
        Command,
        Serial,
        Moderation,
        OvrStream,
        IFTTT,
        Twitch,
        PixelChat,
        [Obsolete]
        VTubeStudio,
    }

    [DataContract]
    public abstract class ActionModelBase
    {
#pragma warning disable CS0612 // Type or member is obsolete
        internal static IEnumerable<ActionModelBase> UpgradeAction(Base.Actions.ActionBase action)
        {
            List<ActionModelBase> actions = new List<ActionModelBase>();
            switch (action.Type)
            {
                case Base.Actions.ActionTypeEnum.Chat:
                    actions.Add(new ChatActionModel((MixItUp.Base.Actions.ChatAction)action));
                    break;
                case Base.Actions.ActionTypeEnum.Clips:
                    actions.Add(new TwitchActionModel((MixItUp.Base.Actions.ClipsAction)action));
                    break;
                case Base.Actions.ActionTypeEnum.Command:
                    actions.Add(new CommandActionModel((MixItUp.Base.Actions.CommandAction)action));
                    break;
                case Base.Actions.ActionTypeEnum.Conditional:
                    MixItUp.Base.Actions.ConditionalAction conAction = (MixItUp.Base.Actions.ConditionalAction)action;
                    ActionModelBase subAction = null;
                    if (conAction.CommandID != Guid.Empty)
                    {
                        CommandActionModel cmdAction = new CommandActionModel(CommandActionTypeEnum.RunCommand, null);
                        cmdAction.CommandID = conAction.CommandID;
                        subAction = cmdAction;
                    }
                    else
                    {
                        IEnumerable<ActionModelBase> subActions = ActionModelBase.UpgradeAction(conAction.Action);
                        if (subActions != null && subActions.Count() > 0)
                        {
                            subAction = subActions.ElementAt(0);
                        }
                    }
                    actions.Add(new ConditionalActionModel(conAction, subAction));
                    break;
                case Base.Actions.ActionTypeEnum.Counter:
                    actions.Add(new CounterActionModel((MixItUp.Base.Actions.CounterAction)action));
                    break;
                case Base.Actions.ActionTypeEnum.Currency:
                    actions.Add(new ConsumablesActionModel((MixItUp.Base.Actions.CurrencyAction)action));
                    break;
                case Base.Actions.ActionTypeEnum.Discord:
                    actions.Add(new DiscordActionModel((MixItUp.Base.Actions.DiscordAction)action));
                    break;
                case Base.Actions.ActionTypeEnum.ExternalProgram:
                    actions.Add(new ExternalProgramActionModel((MixItUp.Base.Actions.ExternalProgramAction)action));
                    break;
                case Base.Actions.ActionTypeEnum.File:
                    actions.Add(new FileActionModel((MixItUp.Base.Actions.FileAction)action));
                    break;
                case Base.Actions.ActionTypeEnum.GameQueue:
                    actions.Add(new GameQueueActionModel((MixItUp.Base.Actions.GameQueueAction)action));
                    break;
                case Base.Actions.ActionTypeEnum.IFTTT:
                    actions.Add(new IFTTTActionModel((MixItUp.Base.Actions.IFTTTAction)action));
                    break;
                case Base.Actions.ActionTypeEnum.Input:
                    actions.Add(new InputActionModel((MixItUp.Base.Actions.InputAction)action));
                    break;
                case Base.Actions.ActionTypeEnum.Moderation:
                    MixItUp.Base.Actions.ModerationAction mAction = (MixItUp.Base.Actions.ModerationAction)action;
                    if (mAction.ModerationType == Base.Actions.ModerationActionTypeEnum.VIPUser || mAction.ModerationType == Base.Actions.ModerationActionTypeEnum.UnVIPUser)
                    {
                        actions.Add(new TwitchActionModel(mAction));
                    }
                    else
                    {
                        actions.Add(new ModerationActionModel(mAction));
                    }
                    break;
                case Base.Actions.ActionTypeEnum.Overlay:
                    actions.Add(new OverlayActionModel((MixItUp.Base.Actions.OverlayAction)action));
                    break;
                case Base.Actions.ActionTypeEnum.OvrStream:
                    actions.Add(new OvrStreamActionModel((MixItUp.Base.Actions.OvrStreamAction)action));
                    break;
                case Base.Actions.ActionTypeEnum.Serial:
                    actions.Add(new SerialActionModel((MixItUp.Base.Actions.SerialAction)action));
                    break;
                case Base.Actions.ActionTypeEnum.Sound:
                    actions.Add(new SoundActionModel((MixItUp.Base.Actions.SoundAction)action));
                    break;
                case Base.Actions.ActionTypeEnum.SpecialIdentifier:
                    actions.Add(new SpecialIdentifierActionModel((MixItUp.Base.Actions.SpecialIdentifierAction)action));
                    break;
                case Base.Actions.ActionTypeEnum.StreamingPlatform:
                    actions.Add(new TwitchActionModel((MixItUp.Base.Actions.StreamingPlatformAction)action));
                    break;
                case Base.Actions.ActionTypeEnum.StreamingSoftware:
                    actions.Add(new StreamingSoftwareActionModel((MixItUp.Base.Actions.StreamingSoftwareAction)action));
                    break;
                case Base.Actions.ActionTypeEnum.Streamlabs:
                    actions.Add(new StreamlabsActionModel((MixItUp.Base.Actions.StreamlabsAction)action));
                    break;
                case Base.Actions.ActionTypeEnum.TextToSpeech:
                    actions.Add(new TextToSpeechActionModel((MixItUp.Base.Actions.TextToSpeechAction)action));
                    break;
                case Base.Actions.ActionTypeEnum.Translation:
                    // Deprecated
                    break;
                case Base.Actions.ActionTypeEnum.Twitter:
                    actions.Add(new TwitterActionModel((MixItUp.Base.Actions.TwitterAction)action));
                    break;
                case Base.Actions.ActionTypeEnum.Wait:
                    actions.Add(new WaitActionModel((MixItUp.Base.Actions.WaitAction)action));
                    break;
                case Base.Actions.ActionTypeEnum.WebRequest:
                    MixItUp.Base.Actions.WebRequestAction wbAction = (MixItUp.Base.Actions.WebRequestAction)action;
                    actions.Add(new WebRequestActionModel(wbAction));
                    if (wbAction.ResponseAction == Base.Actions.WebRequestResponseActionTypeEnum.Chat)
                    {
                        actions.Add(new ChatActionModel(wbAction.ResponseChatText));
                    }
                    else if (wbAction.ResponseAction == Base.Actions.WebRequestResponseActionTypeEnum.Command)
                    {
                        CommandActionModel cmdAction = new CommandActionModel(CommandActionTypeEnum.RunCommand, null);
                        cmdAction.CommandID = wbAction.ResponseCommandID;
                        cmdAction.Arguments = wbAction.ResponseCommandArgumentsText;
                        actions.Add(cmdAction);
                    }
                    else if (wbAction.ResponseAction == Base.Actions.WebRequestResponseActionTypeEnum.SpecialIdentifier)
                    {
                        actions.Add(new SpecialIdentifierActionModel(wbAction.SpecialIdentifierName, "$" + WebRequestActionModel.ResponseSpecialIdentifier, false, false));
                    }
                    break;
            }

            if (actions.Count > 0 && !string.Equals(action.Label, EnumLocalizationHelper.GetLocalizedName(action.Type)))
            {
                actions.First().Name = action.Label;
            }

            return actions;
        }
#pragma warning restore CS0612 // Type or member is obsolete

        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public ActionTypeEnum Type { get; set; }

        [DataMember]
        public bool Enabled { get; set; } = true;

        public ActionModelBase(ActionTypeEnum type)
        {
            this.ID = Guid.NewGuid();
            this.Type = type;
            this.Name = EnumLocalizationHelper.GetLocalizedName(this.Type);
            this.Enabled = true;
        }

        protected ActionModelBase() { }

        public virtual async Task TestPerform(Dictionary<string, string> specialIdentifiers)
        {
            await this.Perform(new CommandParametersModel(ChannelSession.GetCurrentUser(), StreamingPlatformTypeEnum.All, new List<string>() { "@" + ChannelSession.GetCurrentUser().Username }, specialIdentifiers) { TargetUser = ChannelSession.GetCurrentUser() });
        }

        public async Task Perform(CommandParametersModel parameters)
        {
            if (this.Enabled)
            {
                Logger.Log(LogLevel.Debug, $"Starting action performing: {this}");

                ChannelSession.Services.Telemetry.TrackAction(this.Type);

                await this.PerformInternal(parameters);
            }
        }

        protected abstract Task PerformInternal(CommandParametersModel parameters);

        protected async Task<string> ReplaceStringWithSpecialModifiers(string str, CommandParametersModel parameters, bool encode = false)
        {
            return await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(str, parameters, encode);
        }

        public override string ToString() { return string.Format("{0} - {1}", this.ID, this.Name); }

        public int CompareTo(object obj)
        {
            if (obj is ActionModelBase)
            {
                return this.CompareTo((ActionModelBase)obj);
            }
            return 0;
        }

        public int CompareTo(ActionModelBase other) { return this.Name.CompareTo(other.Name); }

        public override bool Equals(object obj)
        {
            if (obj is ActionModelBase)
            {
                return this.Equals((ActionModelBase)obj);
            }
            return false;
        }

        public bool Equals(ActionModelBase other) { return this.ID.Equals(other.ID); }

        public override int GetHashCode() { return this.ID.GetHashCode(); }
    }
}
