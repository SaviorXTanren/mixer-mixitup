using MixItUp.Base.Model.Actions;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
{
    public class LumiaStreamActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.LumiaStream; } }

        public IEnumerable<LumiaStreamActionTypeEnum> ActionTypes { get { return EnumHelper.GetEnumList<LumiaStreamActionTypeEnum>(); } }

        public LumiaStreamActionTypeEnum SelectedActionType
        {
            get { return this.selectedActionType; }
            set
            {
                this.selectedActionType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.ShowCommandsSection));
                this.NotifyPropertyChanged(nameof(this.ShowLightSettingsSection));
            }
        }
        private LumiaStreamActionTypeEnum selectedActionType;

        public bool ShowCommandsSection { get { return this.SelectedActionType == LumiaStreamActionTypeEnum.TriggerCommand; } }

        public IEnumerable<LumiaStreamActionCommandTypeEnum> CommandTypes { get; set; } = EnumHelper.GetEnumList<LumiaStreamActionCommandTypeEnum>();

        public LumiaStreamActionCommandTypeEnum SelectedCommandType
        {
            get { return this.selectedCommandType; }
            set
            {
                this.selectedCommandType = value;
                this.NotifyPropertyChanged();

                this.CommandNames.Clear();
                if (this.commands.ContainsKey(this.SelectedCommandType))
                {
                    this.CommandNames.AddRange(this.commands[this.SelectedCommandType]);
                }
            }
        }
        private LumiaStreamActionCommandTypeEnum selectedCommandType;

        public ObservableCollection<string> CommandNames { get; set; } = new ObservableCollection<string>();

        public string CommandName
        {
            get { return this.commandName; }
            set
            {
                this.commandName = value;
                this.NotifyPropertyChanged();
            }
        }
        private string commandName;

        private Dictionary<LumiaStreamActionCommandTypeEnum, List<string>> commands = new Dictionary<LumiaStreamActionCommandTypeEnum, List<string>>();

        public bool ShowLightSettingsSection { get { return this.SelectedActionType == LumiaStreamActionTypeEnum.SetLightsColor; } }

        public string ColorHex
        {
            get { return this.colorHex; }
            set
            {
                if (!string.IsNullOrEmpty(value) && !value.StartsWith("#"))
                {
                    value = "#" + value;
                }

                this.colorHex = value;
                this.NotifyPropertyChanged();
            }
        }
        private string colorHex = "#FFFFFF";

        public int ColorBrightness
        {
            get { return this.colorBrightness; }
            set
            {
                this.colorBrightness = MathHelper.Clamp(value, 0, 100);
                this.NotifyPropertyChanged();
            }
        }
        private int colorBrightness = 100;

        public double ColorTransition
        {
            get { return this.colorTransition; }
            set
            {
                this.colorTransition = MathHelper.Clamp(value, 0.0, 3.0);
                this.NotifyPropertyChanged();
            }
        }
        private double colorTransition = 0;

        public bool ColorHold
        {
            get { return this.colorHold; }
            set
            {
                this.colorHold = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.ColorNotHold));
            }
        }
        private bool colorHold;

        public bool ColorNotHold { get { return !this.ColorHold; } }

        public double ColorDuration
        {
            get { return this.colorDuration; }
            set
            {
                this.colorDuration = MathHelper.Clamp(value, 0.1, 60.0);
                this.NotifyPropertyChanged();
            }
        }
        private double colorDuration = 5;

        private LumiaStreamActionCommandTypeEnum _previousCommandType;
        private string _previousCommandName;

        public LumiaStreamActionEditorControlViewModel(LumiaStreamActionModel action)
            : base(action)
        {
            this.SelectedActionType = action.ActionType;
            if (this.ShowCommandsSection)
            {
                this._previousCommandType = action.CommandType;
                this._previousCommandName = action.CommandName;
            }
            else if (this.ShowLightSettingsSection)
            {
                this.ColorHex = action.ColorHex;
                this.ColorBrightness = action.ColorBrightness;
                this.ColorTransition = action.ColorTransition;
                this.ColorDuration = action.ColorDuration;
                this.ColorHold = action.ColorHold;
            }
        }

        public LumiaStreamActionEditorControlViewModel() : base() { }

        public override Task<Result> Validate()
        {
            if (this.ShowCommandsSection)
            {
                if (string.IsNullOrEmpty(this.CommandName))
                {
                    return Task.FromResult(new Result(Resources.LumiaStreamActionValidLightMustBeSelected));
                }
            }
            else if (this.ShowLightSettingsSection)
            {
                if (string.IsNullOrEmpty(this.ColorHex))
                {
                    return Task.FromResult(new Result(Resources.LumiaStreamActionValidColorHexMustBeSpecified));
                }
            }

            return Task.FromResult(new Result());
        }

        protected override async Task OnOpenInternal()
        {
            if (ServiceManager.Get<LumiaStreamService>().IsConnected)
            {
                LumiaStreamSettings settings = await ServiceManager.Get<LumiaStreamService>().GetSettings();

                foreach (LumiaStreamActionCommandTypeEnum commandType in this.CommandTypes)
                {
                    this.commands[commandType] = new List<string>();
                    switch (commandType)
                    {
                        case LumiaStreamActionCommandTypeEnum.ChatCommand:
                            this.commands[commandType].AddRange(settings.options.chatCommands.values);
                            break;
                        case LumiaStreamActionCommandTypeEnum.TwitchPoint:
                            this.commands[commandType].AddRange(settings.options.twitchPoints.values);
                            break;
                        case LumiaStreamActionCommandTypeEnum.TwitchExtension:
                            this.commands[commandType].AddRange(settings.options.twitchExtension.values);
                            break;
                        case LumiaStreamActionCommandTypeEnum.TrovoSpell:
                            this.commands[commandType].AddRange(settings.options.trovoSpells.values);
                            break;
                    }
                }

                if (!string.IsNullOrEmpty(this._previousCommandName))
                {
                    this.SelectedCommandType = this._previousCommandType;
                    this.CommandName = this._previousCommandName;
                }
                else
                {
                    this.SelectedCommandType = LumiaStreamActionCommandTypeEnum.ChatCommand;
                }
            }

            await base.OnOpenInternal();
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            if (this.ShowCommandsSection)
            {
                return Task.FromResult<ActionModelBase>(new LumiaStreamActionModel(this.SelectedActionType, this.SelectedCommandType, this.CommandName));
            }
            else if (this.ShowLightSettingsSection)
            {
                return Task.FromResult<ActionModelBase>(new LumiaStreamActionModel(this.SelectedActionType, this.ColorHex, this.ColorBrightness, this.ColorTransition, this.ColorDuration, this.ColorHold));
            }
            return Task.FromResult<ActionModelBase>(null);
        }
    }
}
