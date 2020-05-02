using Mixer.Base.Model.MixPlay;
using MixItUp.Base.Actions;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Commands
{
    public enum MixPlayButtonCommandTriggerType
    {
        [Name("Mouse/Key Down")]
        MouseKeyDown,
        [Name("Mouse/Key Up")]
        MouseKeyUp,
        [Name("Mouse/Key Held")]
        MouseKeyHeld,
    }

    public enum MixPlayJoystickSetupType
    {
        [Name("Directional Arrows")]
        DirectionalArrows,
        WASD,
        [Name("Mouse Movement")]
        MouseMovement,
        [Name("Map To Individual Keys")]
        MapToIndividualKeys,
    }

    public class MixPlayCommand : PermissionsCommandBase
    {
        private static SemaphoreSlim mixPlayCommandPerformSemaphore = new SemaphoreSlim(1);

        [JsonProperty]
        public uint GameID { get; set; }

        [JsonProperty]
        [Obsolete]
        public MixPlayControlModel Control { get; set; }

        [JsonProperty]
        [Obsolete]
        public int IndividualCooldown { get; set; }

        [JsonProperty]
        [Obsolete]
        public string CooldownGroup { get; set; }

        [JsonIgnore]
        public int CooldownAmount
        {
            get
            {
                if (this.Requirements != null && this.Requirements.Cooldown != null)
                {
                    return this.Requirements.Cooldown.CooldownAmount;
                }
                return 0;
            }
        }

        [JsonIgnore]
        public bool HasCooldown { get { return this.CooldownAmount > 0; } }
        
        [JsonIgnore]
        public bool IsIndividualCooldown { get { return (this.Requirements != null && this.Requirements.Cooldown != null) ? this.Requirements.Cooldown.Type == CooldownTypeEnum.Individual : false; } }

        [JsonIgnore]
        public bool IsGroupCooldown { get { return (this.Requirements != null && this.Requirements.Cooldown != null) ? this.Requirements.Cooldown.Type == CooldownTypeEnum.Group : false; } }

        [JsonIgnore]
        public string CooldownGroupName
        {
            get
            {
                if (this.Requirements != null && this.Requirements.Cooldown != null)
                {
                    return this.Requirements.Cooldown.GroupName;
                }
                return string.Empty;
            }
        }

        protected override SemaphoreSlim AsyncSemaphore { get { return MixPlayCommand.mixPlayCommandPerformSemaphore; } }

        public MixPlayCommand() { }

        protected MixPlayCommand(MixPlayGameModel game, MixPlayControlModel control, string command, RequirementViewModel requirements)
            : base(control.controlID, CommandTypeEnum.Interactive, command, requirements)
        {
            this.GameID = game.id;
        }

        [JsonIgnore]
        public virtual string EventTypeString { get { return string.Empty; } }

        public virtual bool DoesInputMatchCommand(MixPlayGiveInputModel input) { return this.EventTypeString.Equals(input.input.eventType); }

        public virtual long GetCooldownTimestamp() { return 0; }

        public void ResetCooldown() { this.Requirements.ResetCooldown(); }
    }

    public class MixPlayButtonCommand : MixPlayCommand
    {
        public const string BasicCommandCooldownGroup = "All Buttons";

        [JsonProperty]
        public MixPlayButtonCommandTriggerType Trigger { get; set; }

        [JsonProperty]
        public int HeldRate { get; set; }

        [JsonIgnore]
        public bool IsBeingHeld { get; set; }

        public MixPlayButtonCommand() { }

        public MixPlayButtonCommand(MixPlayGameModel game, MixPlayButtonControlModel control, MixPlayButtonCommandTriggerType eventType, RequirementViewModel requirements)
            : this(game, control, eventType, 0, requirements)
        { }

        public MixPlayButtonCommand(MixPlayGameModel game, MixPlayButtonControlModel control, MixPlayButtonCommandTriggerType eventType, int heldRate, RequirementViewModel requirements)
            : base(game, control, EnumHelper.GetEnumName(eventType), requirements)
        {
            this.Trigger = eventType;
            this.HeldRate = heldRate;
        }

        [JsonIgnore]
        public override string EventTypeString { get { return this.Trigger.ToString().ToLower(); } }

        public override bool DoesInputMatchCommand(MixPlayGiveInputModel input)
        {
            string inputEvent = input?.input?.eventType;
            if (!string.IsNullOrEmpty(inputEvent))
            {
                if (this.Trigger == MixPlayButtonCommandTriggerType.MouseKeyDown)
                {
                    return inputEvent.Equals("mousedown") || inputEvent.Equals("keydown");
                }
                else if (this.Trigger == MixPlayButtonCommandTriggerType.MouseKeyUp)
                {
                    return inputEvent.Equals("mouseup") || inputEvent.Equals("keyup");
                }
                else if (this.Trigger == MixPlayButtonCommandTriggerType.MouseKeyHeld)
                {
                    if (inputEvent.Equals("mousedown") || inputEvent.Equals("keydown"))
                    {
                        this.IsBeingHeld = true;
                        return true;
                    }
                    else if (inputEvent.Equals("mouseup") || inputEvent.Equals("keyup"))
                    {
                        this.IsBeingHeld = false;
                    }
                }
            }
            return false;
        }

        public override long GetCooldownTimestamp()
        {
            if (this.Requirements.Cooldown != null && (this.Requirements.Cooldown.Type == CooldownTypeEnum.Individual || this.Requirements.Cooldown.Type == CooldownTypeEnum.Group))
            {
                return DateTimeOffset.Now.AddSeconds(this.CooldownAmount).ToUnixTimeMilliseconds();
            }
            return DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers, CancellationToken token)
        {
            if (this.Trigger == MixPlayButtonCommandTriggerType.MouseKeyHeld)
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
    }

    public class MixPlayJoystickCommand : MixPlayCommand
    {
        private class MixPlayJoystickAction : ActionBase
        {
            private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

            protected override SemaphoreSlim AsyncSemaphore { get { return MixPlayJoystickAction.asyncSemaphore; } }

            private MixPlayJoystickCommand command;

            private SemaphoreSlim mouseMovementLock = new SemaphoreSlim(1);
            private Task mouseMovementTask;
            private double mouseMovementX = 0;
            private double mouseMovementY = 0;

            public MixPlayJoystickAction(MixPlayJoystickCommand command) { this.command = command; }

            protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
            {
                if (double.TryParse(arguments.ElementAt(0), out double x) && double.TryParse(arguments.ElementAt(1), out double y))
                {
                    if (this.command.SetupType == MixPlayJoystickSetupType.MouseMovement)
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
                        if (this.command.SetupType == MixPlayJoystickSetupType.DirectionalArrows)
                        {
                            keysToUse.Add(InputKeyEnum.Up);
                            keysToUse.Add(InputKeyEnum.Right);
                            keysToUse.Add(InputKeyEnum.Down);
                            keysToUse.Add(InputKeyEnum.Left);
                        }
                        else if (this.command.SetupType == MixPlayJoystickSetupType.WASD)
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
        public MixPlayJoystickSetupType SetupType { get; set; }

        [JsonProperty]
        public double DeadZone { get; set; }

        [JsonProperty]
        public List<InputKeyEnum?> MappedKeys { get; set; }

        [JsonProperty]
        public double MouseMovementMultiplier { get; set; }

        public MixPlayJoystickCommand()
        {
            this.MappedKeys = new List<InputKeyEnum?>();
        }

        public MixPlayJoystickCommand(MixPlayGameModel game, MixPlayJoystickControlModel control, RequirementViewModel requirements)
            : base(game, control, string.Empty, requirements)
        {
            this.MappedKeys = new List<InputKeyEnum?>();
        }

        [JsonIgnore]
        public override string EventTypeString { get { return "move"; } }

        public void InitializeAction()
        {
            this.Actions.Clear();
            this.Actions.Add(new MixPlayJoystickAction(this));
        }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext streamingContext)
        {
            this.InitializeAction();
        }
    }

    public class MixPlayTextBoxCommand : MixPlayCommand
    {
        public MixPlayTextBoxCommand() { }

        public MixPlayTextBoxCommand(MixPlayGameModel game, MixPlayTextBoxControlModel control, RequirementViewModel requirements)
            : base(game, control, string.Empty, requirements)
        { }

        [JsonProperty]
        public bool UseChatModeration { get; set; }

        [JsonIgnore]
        public override string EventTypeString { get { return "submit"; } }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers, CancellationToken token)
        {
            if (this.UseChatModeration && arguments.Count() > 0)
            {
                if (!string.IsNullOrEmpty(await ChannelSession.Services.Moderation.ShouldTextBeModerated(user, arguments.ElementAt(0))))
                {
                    return;
                }
            }
            await base.PerformInternal(user, arguments, extraSpecialIdentifiers, token);
        }
    }
}
