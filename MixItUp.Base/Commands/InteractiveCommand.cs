using Mixer.Base.Model.Interactive;
using Mixer.Base.Util;
using Newtonsoft.Json;
using System;
using System.Threading;
using MixItUp.Base.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Commands
{
    public enum InteractiveButtonCommandTriggerType
    {
        [Name("Mouse Down")]
        MouseDown,
        [Name("Mouse Up")]
        MouseUp,
        [Name("Key Up")]
        KeyUp,
        [Name("Key Down")]
        KeyDown,
    }

    public class InteractiveCommand : CommandBase
    {
        private static SemaphoreSlim interactiveCommandPerformSemaphore = new SemaphoreSlim(1);

        [JsonProperty]
        public uint GameID { get; set; }

        [JsonProperty]
        public string SceneID { get; set; }

        [JsonProperty]
        public InteractiveControlModel Control { get; set; }

        [JsonProperty]
        public int IndividualCooldown { get; set; }

        [JsonProperty]
        public string CooldownGroup { get; set; }

        [JsonProperty]
        public InteractiveButtonCommandTriggerType Trigger { get; set; }

        public InteractiveCommand() { }

        public InteractiveCommand(InteractiveGameListingModel game, InteractiveSceneModel scene, InteractiveButtonControlModel control, InteractiveButtonCommandTriggerType eventType)
            : base(control.controlID, CommandTypeEnum.Interactive, EnumHelper.GetEnumName(eventType))
        {
            this.GameID = game.id;
            this.SceneID = scene.sceneID;
            this.Control = control;
            this.Trigger = eventType;
        }

        public InteractiveCommand(InteractiveGameListingModel game, InteractiveSceneModel scene, InteractiveJoystickControlModel control)
            : base(control.controlID, CommandTypeEnum.Interactive, string.Empty)
        {
            this.GameID = game.id;
            this.SceneID = scene.sceneID;
            this.Control = control;
        }

        [JsonIgnore]
        public int CooldownAmount
        {
            get
            {
                if (!string.IsNullOrEmpty(this.CooldownGroup) && ChannelSession.Settings.InteractiveCooldownGroups.ContainsKey(this.CooldownGroup))
                {
                    return ChannelSession.Settings.InteractiveCooldownGroups[this.CooldownGroup];
                }
                return this.IndividualCooldown;
            }
        }

        [JsonIgnore]
        public InteractiveButtonControlModel Button { get { return (this.Control is InteractiveButtonControlModel) ? (InteractiveButtonControlModel)this.Control : null; } }

        [JsonIgnore]
        public InteractiveJoystickControlModel Joystick { get { return (this.Control is InteractiveJoystickControlModel) ? (InteractiveJoystickControlModel)this.Control : null; } }

        [JsonIgnore]
        public string TriggerTransactionString { get { return this.Trigger.ToString().ToLower(); } }

        public void UpdateWithLatestControl(InteractiveControlModel control) { this.Control = control; }

        public long GetCooldownTimestamp() { return DateTimeHelper.DateTimeOffsetToUnixTimestamp(DateTimeOffset.Now.AddSeconds(this.CooldownAmount)); }

        protected override SemaphoreSlim AsyncSempahore { get { return InteractiveCommand.interactiveCommandPerformSemaphore; } }
    }
}
