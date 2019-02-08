using Mixer.Base.Model.Interactive;
using Mixer.Base.Util;
using MixItUp.Base.Actions;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Commands
{
    public enum InteractiveButtonCommandTriggerType
    {
        [Name("Mouse/Key Down")]
        MouseKeyDown,
        [Name("Mouse/Key Up")]
        MouseKeyUp,
        [Name("Mouse/Key Held")]
        MouseKeyHeld,
    }

    public enum InteractiveJoystickSetupType
    {
        [Name("Directional Arrows")]
        DirectionalArrows,
        WASD,
        [Name("Mouse Movement")]
        MouseMovement,
        [Name("Map To Individual Keys")]
        MapToIndividualKeys,
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

        protected InteractiveCommand(InteractiveGameModel game, InteractiveSceneModel scene, InteractiveControlModel control, string command, RequirementViewModel requirements)
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

        [JsonProperty]
        public int HeldRate { get; set; }

        [JsonIgnore]
        public bool IsBeingHeld { get; set; }

        public InteractiveButtonCommand() { }

        public InteractiveButtonCommand(InteractiveGameModel game, InteractiveSceneModel scene, InteractiveButtonControlModel control, InteractiveButtonCommandTriggerType eventType, RequirementViewModel requirements)
            : this(game, scene, control, eventType, 0, requirements)
        { }

        public InteractiveButtonCommand(InteractiveGameModel game, InteractiveSceneModel scene, InteractiveButtonControlModel control, InteractiveButtonCommandTriggerType eventType, int heldRate, RequirementViewModel requirements)
            : base(game, scene, control, EnumHelper.GetEnumName(eventType), requirements)
        {
            this.Trigger = eventType;
            this.HeldRate = heldRate;
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
        public bool HasCooldown { get { return this.CooldownAmount > 0; } }

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

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers, CancellationToken token)
        {
            if (this.Trigger == InteractiveButtonCommandTriggerType.MouseKeyHeld)
            {
                do
                {
                    await base.PerformInternal(user, arguments, extraSpecialIdentifiers, token);

                    await Task.Delay(this.HeldRate * 1000);
                } while (this.IsBeingHeld);
            }
            else
            {
                await base.PerformInternal(user, arguments, extraSpecialIdentifiers, token);
            }
        }

        public override async Task<bool> CheckCooldownRequirement(UserViewModel user)
        {
            if (this.Requirements.Cooldown != null && this.Requirements.Cooldown.Type == CooldownTypeEnum.PerPerson)
            {
                return await base.CheckCooldownRequirement(user);
            }
            return true;
        }

        public long GetCooldownTimestamp()
        {
            if (this.Requirements.Cooldown != null && (this.Requirements.Cooldown.Type == CooldownTypeEnum.Individual || this.Requirements.Cooldown.Type == CooldownTypeEnum.Group))
            {
                return DateTimeHelper.DateTimeOffsetToUnixTimestamp(DateTimeOffset.Now.AddSeconds(this.CooldownAmount));
            }
            return DateTimeHelper.DateTimeOffsetToUnixTimestamp(DateTimeOffset.Now);
        }
    }

    public class InteractiveJoystickCommand : InteractiveCommand
    {
        private class InteractiveJoystickAction : ActionBase
        {
            private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

            protected override SemaphoreSlim AsyncSemaphore { get { return InteractiveJoystickAction.asyncSemaphore; } }

            private InteractiveJoystickCommand command;

            private SemaphoreSlim mouseMovementLock = new SemaphoreSlim(1);
            private Task mouseMovementTask;
            private double mouseMovementX = 0;
            private double mouseMovementY = 0;

            public InteractiveJoystickAction(InteractiveJoystickCommand command) { this.command = command; }

            protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
            {
                if (double.TryParse(arguments.ElementAt(0), out double x) && double.TryParse(arguments.ElementAt(1), out double y))
                {
                    if (this.command.SetupType == InteractiveJoystickSetupType.MouseMovement)
                    {
                        if (!(x > command.DeadZone || x < -command.DeadZone)) { x = 0.0; }
                        if (!(y > command.DeadZone || y < -command.DeadZone)) { y = 0.0; }

                        await this.mouseMovementLock.WaitAndRelease(() =>
                        {
                            this.mouseMovementX = x;
                            this.mouseMovementY = y;

                            if (this.mouseMovementTask == null || this.mouseMovementTask.IsCompleted)
                            {
                                this.mouseMovementTask = Task.Run(async () =>
                                {
                                    while (this.mouseMovementX != 0.0 || this.mouseMovementY != 0.0)
                                    {
                                        try
                                        {
                                            ChannelSession.Services.InputService.MoveMouse((int)(this.mouseMovementX * command.MouseMovementMultiplier), (int)(this.mouseMovementY * command.MouseMovementMultiplier));

                                            await Task.Delay(50);
                                        }
                                        catch (Exception) { }
                                    }
                                });
                            }

                            return Task.FromResult(0);
                        });
                    }
                    else
                    {
                        List<InputKeyEnum?> keysToUse = new List<InputKeyEnum?>();
                        if (this.command.SetupType == InteractiveJoystickSetupType.DirectionalArrows)
                        {
                            keysToUse.Add(InputKeyEnum.Up);
                            keysToUse.Add(InputKeyEnum.Right);
                            keysToUse.Add(InputKeyEnum.Down);
                            keysToUse.Add(InputKeyEnum.Left);
                        }
                        else if (this.command.SetupType == InteractiveJoystickSetupType.WASD)
                        {
                            keysToUse.Add(InputKeyEnum.W);
                            keysToUse.Add(InputKeyEnum.A);
                            keysToUse.Add(InputKeyEnum.S);
                            keysToUse.Add(InputKeyEnum.D);
                        }
                        else
                        {
                            keysToUse = this.command.MappedKeys;
                        }

                        if (keysToUse[0] != null)
                        {
                            if (y < -command.DeadZone)
                            {
                                ChannelSession.Services.InputService.KeyDown(keysToUse[0].GetValueOrDefault());
                            }
                            else
                            {
                                ChannelSession.Services.InputService.KeyUp(keysToUse[0].GetValueOrDefault());
                            }
                        }

                        if (keysToUse[2] != null)
                        {
                            if (y > command.DeadZone)
                            {
                                ChannelSession.Services.InputService.KeyDown(keysToUse[2].GetValueOrDefault());
                            }
                            else
                            {
                                ChannelSession.Services.InputService.KeyUp(keysToUse[2].GetValueOrDefault());
                            }
                        }

                        if (keysToUse[1] != null)
                        {
                            if (x < -command.DeadZone)
                            {
                                ChannelSession.Services.InputService.KeyDown(keysToUse[1].GetValueOrDefault());
                            }
                            else
                            {
                                ChannelSession.Services.InputService.KeyUp(keysToUse[1].GetValueOrDefault());
                            }
                        }

                        if (keysToUse[3] != null)
                        {
                            if (x > command.DeadZone)
                            {
                                ChannelSession.Services.InputService.KeyDown(keysToUse[3].GetValueOrDefault());
                            }
                            else
                            {
                                ChannelSession.Services.InputService.KeyUp(keysToUse[3].GetValueOrDefault());
                            }
                        }
                    }
                }
            }
        }

        [JsonProperty]
        public InteractiveJoystickSetupType SetupType { get; set; }

        [JsonProperty]
        public double DeadZone { get; set; }

        [JsonProperty]
        public List<InputKeyEnum?> MappedKeys { get; set; }

        [JsonProperty]
        public double MouseMovementMultiplier { get; set; }

        public InteractiveJoystickCommand()
        {
            this.MappedKeys = new List<InputKeyEnum?>();
        }

        public InteractiveJoystickCommand(InteractiveGameModel game, InteractiveSceneModel scene, InteractiveJoystickControlModel control, RequirementViewModel requirements)
            : base(game, scene, control, string.Empty, requirements)
        {
            this.MappedKeys = new List<InputKeyEnum?>();
        }

        [JsonIgnore]
        public InteractiveJoystickControlModel Joystick { get { return (InteractiveJoystickControlModel)this.Control; } }

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
        public override string EventTypeString { get { return "move"; } }

        public void InitializeAction()
        {
            this.Actions.Clear();
            this.Actions.Add(new InteractiveJoystickAction(this));
        }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext streamingContext)
        {
            this.InitializeAction();
        }
    }

    public class InteractiveTextBoxCommand : InteractiveCommand
    {
        public InteractiveTextBoxCommand() { }

        public InteractiveTextBoxCommand(InteractiveGameModel game, InteractiveSceneModel scene, InteractiveTextBoxControlModel control, RequirementViewModel requirements)
            : base(game, scene, control, string.Empty, requirements)
        { }

        [JsonProperty]
        public bool UseChatModeration { get; set; }

        [JsonIgnore]
        public InteractiveTextBoxControlModel TextBox { get { return (InteractiveTextBoxControlModel)this.Control; } }

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
        public override string EventTypeString { get { return "submit"; } }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers, CancellationToken token)
        {
            if (this.UseChatModeration && arguments.Count() > 0)
            {
                if (!string.IsNullOrEmpty(await ModerationHelper.ShouldBeModerated(user, arguments.ElementAt(0))))
                {
                    return;
                }
            }
            await base.PerformInternal(user, arguments, extraSpecialIdentifiers, token);
        }
    }
}
