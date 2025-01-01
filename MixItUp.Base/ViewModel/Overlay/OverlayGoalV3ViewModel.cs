using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Overlay.Widgets;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayGoalSegmentV3ViewModel : UIViewModelBase
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

        public double Amount
        {
            get { return this.amount; }
            set
            {
                this.amount = Math.Max(value, 1);
                this.NotifyPropertyChanged();
            }
        }
        private double amount = 1;

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

        public ICommand DeleteCommand { get; private set; }

        private OverlayGoalV3ViewModel viewModel;

        public OverlayGoalSegmentV3ViewModel(OverlayGoalV3ViewModel viewModel)
        {
            this.viewModel = viewModel;

            this.Command = this.viewModel.CreateEmbeddedCommand(Resources.SegmentReached);

            this.DeleteCommand = this.CreateCommand(() =>
            {
                this.viewModel.DeleteSegment(this);
            });
        }

        public OverlayGoalSegmentV3ViewModel(OverlayGoalV3ViewModel viewModel, OverlayGoalSegmentV3Model segment)
            : this(viewModel)
        {
            this.Name = segment.Name;
            this.Amount = segment.Amount;

            this.Command = this.viewModel.GetEmbeddedCommand(segment.CommandID, Resources.SegmentReached);
        }
    }

    public class OverlayGoalV3ViewModel : OverlayEventTrackingV3ViewModelBase
    {
        public override string DefaultHTML { get { return OverlayGoalV3Model.DefaultHTML; } }
        public override string DefaultCSS { get { return OverlayGoalV3Model.DefaultCSS; } }
        public override string DefaultJavascript { get { return OverlayGoalV3Model.DefaultJavascript; } }

        public override bool IsTestable { get { return true; } }

        public override string EquationUnits { get { return Resources.Progress; } }

        public IEnumerable<OverlayGoalV3Type> GoalTypes { get; set; } = EnumHelper.GetEnumList<OverlayGoalV3Type>();

        public OverlayGoalV3Type SelectedGoalType
        {
            get { return this.selectedGoalType; }
            set
            {
                this.selectedGoalType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.ShowCustomSelections));
                this.NotifyPropertyChanged(nameof(this.ShowCounterSelections));
                this.NotifyPropertyChanged(nameof(this.ShowStreamingPlatformSelections));
            }
        }
        private OverlayGoalV3Type selectedGoalType = OverlayGoalV3Type.Custom;

        public bool ShowCustomSelections { get { return this.SelectedGoalType == OverlayGoalV3Type.Custom; } }

        public bool ShowCounterSelections { get { return this.SelectedGoalType == OverlayGoalV3Type.Counter; } }

        public bool ShowStreamingPlatformSelections { get { return this.SelectedGoalType == OverlayGoalV3Type.Followers || this.SelectedGoalType == OverlayGoalV3Type.Subscribers; } }

        public string Height
        {
            get { return this.height > 0 ? this.height.ToString() : string.Empty; }
            set
            {
                this.height = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        protected int height;

        public string BorderColor
        {
            get { return this.borderColor; }
            set
            {
                this.borderColor = value;
                this.NotifyPropertyChanged();
            }
        }
        private string borderColor;

        public string GoalColor
        {
            get { return this.goalColor; }
            set
            {
                this.goalColor = value;
                this.NotifyPropertyChanged();
            }
        }
        private string goalColor;

        public string ProgressColor
        {
            get { return this.progressColor; }
            set
            {
                this.progressColor = value;
                this.NotifyPropertyChanged();
            }
        }
        private string progressColor;

        public ResetTrackerViewModel ResetTracker
        {
            get { return this.resetTracker; }
            set
            {
                this.resetTracker = value;
                this.NotifyPropertyChanged();
            }
        }
        private ResetTrackerViewModel resetTracker;

        public ObservableCollection<CounterModel> Counters { get; set; } = new ObservableCollection<CounterModel>();
        public CounterModel SelectedCounter
        {
            get { return this.selectedCounter; }
            set
            {
                this.selectedCounter = value;
                this.NotifyPropertyChanged();
            }
        }
        private CounterModel selectedCounter;

        public ObservableCollection<StreamingPlatformTypeEnum> StreamingPlatforms { get; set; } = new ObservableCollection<StreamingPlatformTypeEnum>() { StreamingPlatformTypeEnum.Twitch };
        public StreamingPlatformTypeEnum SelectedStreamingPlatform
        {
            get { return this.selectedStreamingPlatform; }
            set
            {
                this.selectedStreamingPlatform = value;
                this.NotifyPropertyChanged();
            }
        }
        private StreamingPlatformTypeEnum selectedStreamingPlatform = StreamingPlatformTypeEnum.Twitch;

        public double StartingAmountCustom
        {
            get { return this.startingAmountCustom; }
            set
            {
                this.startingAmountCustom = value;
                this.NotifyPropertyChanged();
            }
        }
        private double startingAmountCustom;

        public IEnumerable<OverlayGoalSegmentV3Type> SegmentTypes { get; set; } = EnumHelper.GetEnumList<OverlayGoalSegmentV3Type>();

        public OverlayGoalSegmentV3Type SelectedSegmentType
        {
            get { return this.selectedSegmentType; }
            set
            {
                this.selectedSegmentType = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayGoalSegmentV3Type selectedSegmentType = OverlayGoalSegmentV3Type.Cumulative;

        public ObservableCollection<OverlayGoalSegmentV3ViewModel> Segments { get; set; } = new ObservableCollection<OverlayGoalSegmentV3ViewModel>();

        public CustomCommandModel ProgressOccurredCommand
        {
            get { return this.progressOccurredCommand; }
            set
            {
                this.progressOccurredCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel progressOccurredCommand;

        public CustomCommandModel SegmentCompletedCommand
        {
            get { return this.segmentCompletedCommand; }
            set
            {
                this.segmentCompletedCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel segmentCompletedCommand;

        public ICommand AddSegmentCommand { get; private set; }

        public OverlayAnimationV3ViewModel ProgressOccurredAnimation;
        public OverlayAnimationV3ViewModel SegmentCompletedAnimation;

        public OverlayGoalV3ViewModel()
            : base(OverlayItemV3Type.Goal)
        {
            this.SelectedGoalType = OverlayGoalV3Type.Custom;

            this.FontSize = 16;

            this.Width = "400";
            this.Height = "100";

            this.BorderColor = "Black";
            this.GoalColor = "Red";
            this.ProgressColor = "Green";

            this.ResetTracker = new ResetTrackerViewModel();

            this.Segments.Add(new OverlayGoalSegmentV3ViewModel(this)
            {
                Name = "My Cool Goal",
                Amount = 1000
            });

            this.ProgressOccurredCommand = this.CreateEmbeddedCommand(Resources.ProgressOccurred);
            this.SegmentCompletedCommand = this.CreateEmbeddedCommand(Resources.SegmentCompleted);

            this.ProgressOccurredAnimation = new OverlayAnimationV3ViewModel(Resources.ProgressOccurred, new OverlayAnimationV3Model());
            this.SegmentCompletedAnimation = new OverlayAnimationV3ViewModel(Resources.SegmentCompleted, new OverlayAnimationV3Model());

            this.Animations.Add(this.ProgressOccurredAnimation);
            this.Animations.Add(this.SegmentCompletedAnimation);

            this.Counters.AddRange(ChannelSession.Settings.Counters.Values);

            this.InitializeInternal();
        }

        public OverlayGoalV3ViewModel(OverlayGoalV3Model item)
            : base(item)
        {
            this.SelectedGoalType = item.GoalType;

            this.height = item.Height;

            this.BorderColor = item.BorderColor;
            this.GoalColor = item.GoalColor;
            this.ProgressColor = item.ProgressColor;

            this.StartingAmountCustom = item.StartingAmountCustom;
            this.SelectedSegmentType = item.SegmentType;

            this.ResetTracker = new ResetTrackerViewModel(item.ResetTracker);

            foreach (OverlayGoalSegmentV3Model segment in item.Segments)
            {
                this.Segments.Add(new OverlayGoalSegmentV3ViewModel(this, segment));
            }

            this.ProgressOccurredCommand = this.GetEmbeddedCommand(item.ProgressOccurredCommandID, Resources.ProgressOccurred);
            this.SegmentCompletedCommand = this.GetEmbeddedCommand(item.SegmentCompletedCommandID, Resources.SegmentCompleted);

            this.ProgressOccurredAnimation = new OverlayAnimationV3ViewModel(Resources.ProgressOccurred, item.ProgressOccurredAnimation);
            this.SegmentCompletedAnimation = new OverlayAnimationV3ViewModel(Resources.SegmentCompleted, item.SegmentCompletedAnimation);

            this.Animations.Add(this.ProgressOccurredAnimation);
            this.Animations.Add(this.SegmentCompletedAnimation);

            this.Counters.AddRange(ChannelSession.Settings.Counters.Values);
            if (this.ShowCounterSelections)
            {
                this.SelectedCounter = this.Counters.FirstOrDefault(c => string.Equals(c.Name, item.CounterName, StringComparison.OrdinalIgnoreCase));
            }
            else if (this.ShowStreamingPlatformSelections)
            {
                this.SelectedStreamingPlatform = item.StreamingPlatform;
            }

            this.InitializeInternal();
        }

        public void DeleteSegment(OverlayGoalSegmentV3ViewModel segment)
        {
            this.Segments.Remove(segment);
        }

        public override Result Validate()
        {
            if (this.Segments.Count == 0)
            {
                return new Result(Resources.OverlayGoalAtLeastOneSegmentMustBeAdded);
            }

            if (this.SelectedGoalType == OverlayGoalV3Type.Counter && this.SelectedCounter == null)
            {
                return new Result(Resources.OverlayGoalValidCounterMustBeSelected);
            }

            return new Result();
        }

        public override async Task TestWidget(OverlayWidgetV3Model widget)
        {
            OverlayGoalV3Model goal = (OverlayGoalV3Model)widget.Item;

            await goal.ProcessEvent(ChannelSession.User, goal.CurrentSegment.Amount / 2);

            await base.TestWidget(widget);
        }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            OverlayGoalV3Model result = new OverlayGoalV3Model();

            this.AssignProperties(result);

            result.GoalType = this.SelectedGoalType;

            result.Height = this.height;

            result.BorderColor = this.BorderColor;
            result.GoalColor = this.GoalColor;
            result.ProgressColor = this.ProgressColor;

            result.ResetTracker = this.ResetTracker.Model;
            result.StartingAmountCustom = this.StartingAmountCustom;
            result.SegmentType = this.SelectedSegmentType;

            result.Segments.Clear();
            foreach (OverlayGoalSegmentV3ViewModel segment in this.Segments)
            {
                result.Segments.Add(new OverlayGoalSegmentV3Model()
                {
                    Name = segment.Name,
                    Amount = segment.Amount,
                    CommandID = segment.Command.ID,
                });

                ChannelSession.Settings.SetCommand(segment.Command);
            }

            result.ProgressOccurredCommandID = this.ProgressOccurredCommand.ID;
            ChannelSession.Settings.SetCommand(this.ProgressOccurredCommand);

            result.SegmentCompletedCommandID = this.SegmentCompletedCommand.ID;
            ChannelSession.Settings.SetCommand(this.SegmentCompletedCommand);

            result.ProgressOccurredAnimation = this.ProgressOccurredAnimation.GetAnimation();
            result.SegmentCompletedAnimation = this.SegmentCompletedAnimation.GetAnimation();

            if (this.SelectedGoalType == OverlayGoalV3Type.Counter)
            {
                result.CounterName = this.SelectedCounter.Name;

                result.ClearAllAmountsToZero();
            }
            else if (this.SelectedGoalType == OverlayGoalV3Type.Followers || this.SelectedGoalType == OverlayGoalV3Type.Subscribers)
            {
                result.StreamingPlatform = this.SelectedStreamingPlatform;

                result.ClearAllAmountsToZero();
            }

            return result;
        }

        private void InitializeInternal()
        {
            this.AddSegmentCommand = this.CreateCommand(() =>
            {
                OverlayGoalSegmentV3ViewModel segment = new OverlayGoalSegmentV3ViewModel(this);
                segment.PropertyChanged += (sender, e) =>
                {
                    this.NotifyPropertyChanged("X");
                };
                this.Segments.Add(segment);
            });

            foreach (OverlayGoalSegmentV3ViewModel segment in this.Segments)
            {
                segment.PropertyChanged += (sender, e) =>
                {
                    this.NotifyPropertyChanged("X");
                };
            }
        }
    }
}
