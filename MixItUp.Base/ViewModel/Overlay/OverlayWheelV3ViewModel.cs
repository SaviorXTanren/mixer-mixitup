using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Overlay.Widgets;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
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

        public double Percentage
        {
            get { return this.percentage; }
            set
            {
                this.percentage = value;
                this.NotifyPropertyChanged();
            }
        }
        private double percentage;

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

        public ICommand DeleteCommand { get; private set; }

        private OverlayWheelV3ViewModel wheel;

        public OverlayWheelOutcomeV3ViewModel(OverlayWheelV3ViewModel wheel)
        {
            this.wheel = wheel;

            this.Initialize();
        }

        public OverlayWheelOutcomeV3ViewModel(OverlayWheelV3ViewModel wheel, OverlayWheelOutcomeV3Model slice)
        {
            this.wheel = wheel;

            this.Name = slice.Name;
            this.Percentage = slice.Percentage;
            this.Color = slice.Color;

            this.Initialize();
        }

        public Result Validate()
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                return new Result(Resources.OverlayWheelOutcomeMissingName);
            }

            if (this.Percentage < 0 ||
                this.Percentage > 100)
            {
                return new Result(Resources.OverlayWheelOutcomeInvalidProbability);
            }

            return new Result();
        }

        public OverlayWheelOutcomeV3Model GetModel()
        {
            return new OverlayWheelOutcomeV3Model()
            {
                Name = this.Name,
                Percentage = this.Percentage,
                Color = this.Color,
            };
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

        public ObservableCollection<OverlayWheelOutcomeV3ViewModel> Outcomes { get; set; } = new ObservableCollection<OverlayWheelOutcomeV3ViewModel>();

        public ICommand AddOutcomeCommand { get; set; }

        public OverlayWheelV3ViewModel()
            : base(OverlayItemV3Type.Wheel)
        {
            this.Width = "300";
            this.FontSize = 40;
            this.FontColor = "Black";

            this.Initialize();
        }

        public OverlayWheelV3ViewModel(OverlayWheelV3Model item)
            : base(item)
        {
            foreach (OverlayWheelOutcomeV3Model outcome in item.Outcomes)
            {
                OverlayWheelOutcomeV3ViewModel outcomeVM = new OverlayWheelOutcomeV3ViewModel(this, outcome);
                this.Outcomes.Add(outcomeVM);
                outcomeVM.PropertyChanged += (sender, e) =>
                {
                    this.NotifyPropertyChanged("X");
                };
            }

            this.Initialize();
        }

        public override Result Validate()
        {
            double totalPercentage = 0.0;
            foreach (OverlayWheelOutcomeV3ViewModel outcome in this.Outcomes)
            {
                Result result = outcome.Validate();
                if (!result.Success)
                {
                    return result;
                }

                totalPercentage += outcome.Percentage;
            }

            if (totalPercentage != 100.0)
            {
                return new Result(Resources.OverlayWheelOutcomeTotalPercentageMustEqual100);
            }

            return new Result();
        }

        public override async Task TestWidget(OverlayWidgetV3Model widget)
        {
            OverlayWheelV3Model wheel = (OverlayWheelV3Model)widget.Item;

            await wheel.Spin(new CommandParametersModel());

            await base.TestWidget(widget);
        }

        public void DeleteOutcome(OverlayWheelOutcomeV3ViewModel outcome)
        {
            this.Outcomes.Remove(outcome);
        }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            OverlayWheelV3Model result = new OverlayWheelV3Model();

            this.AssignProperties(result);

            foreach (OverlayWheelOutcomeV3ViewModel outcome in this.Outcomes)
            {
                result.Outcomes.Add(outcome.GetModel());
            }

            return result;
        }

        private void Initialize()
        {
            this.AddOutcomeCommand = this.CreateCommand(() =>
            {
                OverlayWheelOutcomeV3ViewModel outcome = new OverlayWheelOutcomeV3ViewModel(this);
                this.Outcomes.Add(outcome);
                outcome.PropertyChanged += (sender, e) =>
                {
                    this.NotifyPropertyChanged("X");
                };
            });
        }
    }
}
