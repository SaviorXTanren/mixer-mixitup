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

        protected override SemaphoreSlim AsyncSemaphore { get { return InteractiveCommand.interactiveCommandPerformSemaphore; } }

        public InteractiveCommand() { }

        protected InteractiveCommand(InteractiveGameListingModel game, InteractiveSceneModel scene, InteractiveControlModel control, string command, RequirementViewModel requirements)
            : base(control.controlID, CommandTypeEnum.Interactive, command, requirements)
        {
            this.GameID = game.id;
            this.SceneID = scene.sceneID;
            this.Control = control;
        }

        [JsonIgnore]
        public virtual string EventTypeString { get { return string.Empty; } }

        public void UpdateWithLatestControl(InteractiveControlModel control) { this.Control = control; }
    }

    public class InteractiveButtonCommand : InteractiveCommand
    {
        public const string BasicCommandCooldownGroup = "All Buttons";

        [JsonProperty]
        public InteractiveButtonCommandTriggerType Trigger { get; set; }

        public InteractiveButtonCommand() { }

        public InteractiveButtonCommand(InteractiveGameListingModel game, InteractiveSceneModel scene, InteractiveButtonControlModel control, InteractiveButtonCommandTriggerType eventType, RequirementViewModel requirements)
            : base(game, scene, control, EnumHelper.GetEnumName(eventType), requirements)
        {
            this.Trigger = eventType;
        }

        [JsonIgnore]
        public InteractiveButtonControlModel Button { get { return (InteractiveButtonControlModel)this.Control; } }

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
        public override string EventTypeString { get { return this.Trigger.ToString().ToLower(); } }

        public long GetCooldownTimestamp()
        {
            if (this.Requirements.Cooldown != null && (this.Requirements.Cooldown.Type == CooldownTypeEnum.Global || this.Requirements.Cooldown.Type == CooldownTypeEnum.Group))
            {
                return DateTimeHelper.DateTimeOffsetToUnixTimestamp(DateTimeOffset.Now.AddSeconds(this.CooldownAmount));
            }
            return DateTimeHelper.DateTimeOffsetToUnixTimestamp(DateTimeOffset.Now);
        }
    }

    public class InteractiveJoystickCommand : InteractiveCommand
    {
        public InteractiveJoystickCommand() { }

        public InteractiveJoystickCommand(InteractiveGameListingModel game, InteractiveSceneModel scene, InteractiveJoystickControlModel control, RequirementViewModel requirements)
            : base(game, scene, control, string.Empty, requirements)
        { }

        [JsonIgnore]
        public InteractiveJoystickControlModel Joystick { get { return (InteractiveJoystickControlModel)this.Control; } }

        [JsonIgnore]
        public override string EventTypeString { get { return "move"; } }
    }
}
