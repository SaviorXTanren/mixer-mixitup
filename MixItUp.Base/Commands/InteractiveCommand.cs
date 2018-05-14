using Mixer.Base.Model.Interactive;
using Mixer.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using Newtonsoft.Json;
using System;
using System.Threading;

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

    public class InteractiveCommand : PermissionsCommandBase
    {
        public const string BasicCommandCooldownGroup = "All Buttons";

        private static SemaphoreSlim interactiveCommandPerformSemaphore = new SemaphoreSlim(1);

        [JsonProperty]
        public uint GameID { get; set; }

        [JsonProperty]
        public string SceneID { get; set; }

        [JsonProperty]
        public InteractiveControlModel Control { get; set; }

        [JsonProperty]
        [Obsolete]
        public int IndividualCooldown { get; set; }

        [JsonProperty]
        [Obsolete]
        public string CooldownGroup { get; set; }

        [JsonProperty]
        public InteractiveButtonCommandTriggerType Trigger { get; set; }

        public InteractiveCommand() { }

        public InteractiveCommand(InteractiveGameListingModel game, InteractiveSceneModel scene, InteractiveButtonControlModel control, InteractiveButtonCommandTriggerType eventType, RequirementViewModel requirements)
            : base(control.controlID, CommandTypeEnum.Interactive, EnumHelper.GetEnumName(eventType), requirements)
        {
            this.GameID = game.id;
            this.SceneID = scene.sceneID;
            this.Control = control;
            this.Trigger = eventType;
        }

        public InteractiveCommand(InteractiveGameListingModel game, InteractiveSceneModel scene, InteractiveJoystickControlModel control, RequirementViewModel requirements)
            : base(control.controlID, CommandTypeEnum.Interactive, string.Empty, requirements)
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
                if (this.Requirements.Cooldown != null)
                {
                    return this.Requirements.Cooldown.CooldownAmount;
                }
                return 0;
            }
        }

        [JsonIgnore]
        public string CooldownGroupName
        {
            get
            {
                if (this.Requirements.Cooldown != null)
                {
                    return this.Requirements.Cooldown.GroupName;
                }
                return string.Empty;
            }
        }

        [JsonIgnore]
        public InteractiveButtonControlModel Button { get { return (this.Control is InteractiveButtonControlModel) ? (InteractiveButtonControlModel)this.Control : null; } }

        [JsonIgnore]
        public InteractiveJoystickControlModel Joystick { get { return (this.Control is InteractiveJoystickControlModel) ? (InteractiveJoystickControlModel)this.Control : null; } }

        [JsonIgnore]
        public string TriggerTransactionString { get { return this.Trigger.ToString().ToLower(); } }

        public void UpdateWithLatestControl(InteractiveControlModel control) { this.Control = control; }

        public long GetCooldownTimestamp()
        {
            if (this.Requirements.Cooldown != null && (this.Requirements.Cooldown.Type == CooldownTypeEnum.Global || this.Requirements.Cooldown.Type == CooldownTypeEnum.Group))
            {
                return DateTimeHelper.DateTimeOffsetToUnixTimestamp(DateTimeOffset.Now.AddSeconds(this.CooldownAmount));
            }
            return DateTimeHelper.DateTimeOffsetToUnixTimestamp(DateTimeOffset.Now);
        }

        protected override SemaphoreSlim AsyncSemaphore { get { return InteractiveCommand.interactiveCommandPerformSemaphore; } }
    }
}
