using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Overlay.Widgets;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayWheelOutcomeV3ViewModel : UIViewModelBase
    {
        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                this.NotifyPropertyChanged();
            }
        }
        private string name;

        public double Probability
        {
            get { return this.probability; }
            set
            {
                this.probability = value;
                this.NotifyPropertyChanged();
                this.wheel.UpdateTotalProbability();
            }
        }
        private double probability = 0.0;

        public double Modifier
        {
            get { return this.modifier; }
            set
            {
                this.modifier = value;
                this.NotifyPropertyChanged();
                this.wheel.UpdateTotalModifier();
            }
        }
        private double modifier = 0;

        public string Color
        {
            get { return this.color; }
            set
            {
                this.color = value;
                this.NotifyPropertyChanged();
            }
        }
        private string color;

        public CustomCommandModel Command
        {
            get { return this.command; }
            set
            {
                this.command = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel command;

        public bool CanSetProbabilities { get { return !this.wheel.EqualProbabilityForOutcomes; } }

        public ICommand DeleteCommand { get; private set; }

        private OverlayWheelV3ViewModel wheel;

        public OverlayWheelOutcomeV3ViewModel(OverlayWheelV3ViewModel wheel)
        {
            this.wheel = wheel;

            this.Command = this.wheel.CreateEmbeddedCommand(Resources.OutcomeSelected);

            this.Initialize();
        }

        public OverlayWheelOutcomeV3ViewModel(OverlayWheelV3ViewModel wheel, OverlayWheelOutcomeV3Model outcome)
        {
            this.wheel = wheel;

            this.Name = outcome.Name;
            this.Probability = outcome.Probability;
            this.modifier = outcome.Modifier;
            this.Color = outcome.Color;

            this.Command = this.wheel.GetEmbeddedCommand(outcome.CommandID, Resources.OutcomeSelected);

            this.Initialize();
        }

        public Result Validate()
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                return new Result(Resources.OverlayWheelOutcomeMissingName);
            }

            if (this.Probability < 0 ||
                this.Probability > 100)
            {
                return new Result(Resources.OverlayWheelOutcomeInvalidProbability);
            }

            return new Result();
        }

        public OverlayWheelOutcomeV3Model GetModel()
        {
            OverlayWheelOutcomeV3Model result = new OverlayWheelOutcomeV3Model()
            {
                Name = this.Name,
                Probability = this.Probability,
                Modifier = this.Modifier,
                Color = this.Color,
                CommandID = this.Command.ID,
            };

            ChannelSession.Settings.SetCommand(this.Command);

            return result;
        }

        public void UpdateCanSetProbabilities()
        {
            this.NotifyPropertyChanged(nameof(this.CanSetProbabilities));
        }

        private void Initialize()
        {
            this.DeleteCommand = this.CreateCommand(() =>
            {
                this.wheel.DeleteOutcome(this);
            });
        }
    }

    public class OverlayWheelV3ViewModel : OverlayVisualTextV3ViewModelBase
    {
        public override string DefaultHTML { get { return OverlayWheelV3Model.DefaultHTML; } }
        public override string DefaultCSS { get { return OverlayWheelV3Model.DefaultCSS; } }
        public override string DefaultJavascript { get { return OverlayWheelV3Model.DefaultJavascript; } }

        public override bool IsTestable { get { return true; } }

        public int Size
        {
            get { return this.size; }
            set
            {
                this.size = (value > 0) ? value : 0;
                this.NotifyPropertyChanged();
            }
        }
        private int size;

        public string WheelClickSoundFilePath
        {
            get { return this.wheelClickSoundFilePath; }
            set
            {
                this.wheelClickSoundFilePath = value;
                this.NotifyPropertyChanged();
            }
        }
        private string wheelClickSoundFilePath;

        public int WheelClickVolume
        {
            get { return this.wheelClickVolume; }
            set
            {
                this.wheelClickVolume = value;
                this.NotifyPropertyChanged();
            }
        }
        private int wheelClickVolume = 100;

        public CustomCommandModel DefaultOutcomeCommand
        {
            get { return this.defaultOutcomeCommand; }
            set
            {
                this.defaultOutcomeCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel defaultOutcomeCommand;

        public bool EqualProbabilityForOutcomes
        {
            get { return this.equalProbabilityForOutcomes; }
            set
            {
                this.equalProbabilityForOutcomes = value;
                this.NotifyPropertyChanged();

                foreach (OverlayWheelOutcomeV3ViewModel outcome in this.Outcomes)
                {
                    outcome.UpdateCanSetProbabilities();
                }
            }
        }
        private bool equalProbabilityForOutcomes = true;

        public ObservableCollection<OverlayWheelOutcomeV3ViewModel> Outcomes { get; set; } = new ObservableCollection<OverlayWheelOutcomeV3ViewModel>();

        public string TotalProbability
        {
            get
            {
                double total = 0.0;
                foreach (OverlayWheelOutcomeV3ViewModel outcome in this.Outcomes)
                {
                    total += outcome.Probability;
                }
                return total + " %";
            }
        }

        public string TotalModifier
        {
            get
            {
                double total = 0.0;
                foreach (OverlayWheelOutcomeV3ViewModel outcome in this.Outcomes)
                {
                    total += outcome.Modifier;
                }

                if (total > 0) { return $"+ {total}"; }
                else if (total < 0) { return $"- {Math.Abs(total)}"; }
                else { return "0"; }
            }
        }

        public OverlayAnimationV3ViewModel EntranceAnimation;
        public OverlayAnimationV3ViewModel OutcomeSelectedAnimation;
        public OverlayAnimationV3ViewModel ExitAnimation;

        public ICommand BrowseFilePathCommand { get; set; }

        public ICommand AddOutcomeCommand { get; set; }

        public OverlayWheelV3ViewModel()
            : base(OverlayItemV3Type.Wheel)
        {
            this.FontSize = 40;
            this.FontColor = "Black";

            this.Size = 600;
            this.WheelClickSoundFilePath = OverlayWheelV3Model.DefaultWheelClickSoundFilePath;

            this.DefaultOutcomeCommand = this.CreateEmbeddedCommand(Resources.DefaultOutcome);
            this.DefaultOutcomeCommand.Actions.Add(new ChatActionModel("Outcome Selected: $outcomename"));

            this.EqualProbabilityForOutcomes = true;

            this.AddOutcome(new OverlayWheelOutcomeV3ViewModel(this)
            {
                Name = "Yes",
                Probability = 50,
            });

            this.AddOutcome(new OverlayWheelOutcomeV3ViewModel(this)
            {
                Name = "No",
                Probability = 50,
            });

            this.EntranceAnimation = new OverlayAnimationV3ViewModel(Resources.Entrance, new OverlayAnimationV3Model());
            this.OutcomeSelectedAnimation = new OverlayAnimationV3ViewModel(Resources.OutcomeSelected, new OverlayAnimationV3Model());
            this.ExitAnimation = new OverlayAnimationV3ViewModel(Resources.Exit, new OverlayAnimationV3Model());

            this.Initialize();
        }

        public OverlayWheelV3ViewModel(OverlayWheelV3Model item)
            : base(item)
        {
            this.Size = item.Size;
            this.WheelClickSoundFilePath = item.WheelClickSoundFilePath;
            this.WheelClickVolume = (int)(item.WheelClickVolume * 100);

            this.DefaultOutcomeCommand = this.GetEmbeddedCommand(item.DefaultOutcomeCommand, Resources.DefaultOutcome);

            this.EqualProbabilityForOutcomes = item.EqualProbabilityForOutcomes;

            foreach (OverlayWheelOutcomeV3Model outcome in item.Outcomes)
            {
                this.AddOutcome(new OverlayWheelOutcomeV3ViewModel(this, outcome));
            }

            this.EntranceAnimation = new OverlayAnimationV3ViewModel(Resources.Entrance, item.EntranceAnimation);
            this.OutcomeSelectedAnimation = new OverlayAnimationV3ViewModel(Resources.OutcomeSelected, item.OutcomeSelectedAnimation);
            this.ExitAnimation = new OverlayAnimationV3ViewModel(Resources.Exit, item.ExitAnimation);

            this.Initialize();
        }

        public override Result Validate()
        {
            if (this.Size <= 0)
            {
                return new Result(Resources.OverlayWheelSizeMustBeGreaterThan0);
            }

            if (!this.EqualProbabilityForOutcomes)
            {
                double totalPercentage = 0.0;
                double totalModifier = 0.0;
                foreach (OverlayWheelOutcomeV3ViewModel outcome in this.Outcomes)
                {
                    Result result = outcome.Validate();
                    if (!result.Success)
                    {
                        return result;
                    }

                    totalPercentage += outcome.Probability;
                    totalModifier += outcome.Modifier;
                }

                if (totalPercentage != 100.0)
                {
                    return new Result(Resources.OverlayWheelOutcomeTotalPercentageMustEqual100);
                }

                if (totalModifier != 0.0)
                {
                    return new Result(Resources.OverlayWheelOutcomeTotalModifierMustEqual0);
                }
            }

            return new Result();
        }

        public override async Task TestWidget(OverlayWidgetV3Model widget)
        {
            await base.TestWidget(widget);

            OverlayWheelV3Model wheel = (OverlayWheelV3Model)widget.Item;

            await wheel.Spin(new CommandParametersModel());
        }

        public void UpdateTotalProbability()
        {
            this.NotifyPropertyChanged(nameof(this.TotalProbability));
        }

        public void UpdateTotalModifier()
        {
            this.NotifyPropertyChanged(nameof(this.TotalModifier));
        }

        public void DeleteOutcome(OverlayWheelOutcomeV3ViewModel outcome)
        {
            this.Outcomes.Remove(outcome);
        }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            OverlayWheelV3Model result = new OverlayWheelV3Model();

            this.AssignProperties(result);

            result.Size = this.Size;
            result.WheelClickSoundFilePath = this.WheelClickSoundFilePath;
            result.WheelClickVolume = ((double)this.WheelClickVolume) / 100.0;

            result.DefaultOutcomeCommand = this.DefaultOutcomeCommand.ID;
            ChannelSession.Settings.SetCommand(this.DefaultOutcomeCommand);

            result.EqualProbabilityForOutcomes = this.EqualProbabilityForOutcomes;

            foreach (OverlayWheelOutcomeV3ViewModel outcome in this.Outcomes)
            {
                result.Outcomes.Add(outcome.GetModel());
            }

            foreach (OverlayWheelOutcomeV3Model outcome in result.Outcomes)
            {
                if (!string.IsNullOrEmpty(outcome.Name) && string.IsNullOrWhiteSpace(outcome.Color))
                {
                    outcome.Color = OverlayItemV3ModelBase.GetRandomHTMLColor(outcome.Name);
                }
            }

            result.EntranceAnimation = this.EntranceAnimation.GetAnimation();
            result.OutcomeSelectedAnimation = this.OutcomeSelectedAnimation.GetAnimation();
            result.ExitAnimation = this.ExitAnimation.GetAnimation();

            return result;
        }

        private void Initialize()
        {
            this.Animations.Add(this.EntranceAnimation);
            this.Animations.Add(this.OutcomeSelectedAnimation);
            this.Animations.Add(this.ExitAnimation);

            this.AddOutcomeCommand = this.CreateCommand(() =>
            {
                this.AddOutcome(new OverlayWheelOutcomeV3ViewModel(this));
            });

            this.BrowseFilePathCommand = this.CreateCommand(() =>
            {
                string filepath = ServiceManager.Get<IFileService>().ShowOpenFileDialog(ServiceManager.Get<IFileService>().SoundFileFilter());
                if (!string.IsNullOrWhiteSpace(filepath))
                {
                    this.WheelClickSoundFilePath = filepath;
                }
            });
        }

        private void AddOutcome(OverlayWheelOutcomeV3ViewModel outcome)
        {
            this.Outcomes.Add(outcome);
            outcome.PropertyChanged += (sender, e) =>
            {
                this.NotifyPropertyChanged("X");
            };
        }
    }
}
