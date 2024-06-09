using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayPollHeaderV3ViewModel : OverlayHeaderV3ViewModelBase
    {
        public OverlayPollHeaderV3ViewModel() { }

        public OverlayPollHeaderV3ViewModel(OverlayPollHeaderV3Model model) : base(model) { }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            OverlayPollHeaderV3Model result = new OverlayPollHeaderV3Model();
            this.AssignProperties(result);
            return result;
        }
    }

    public class OverlayPollV3ViewModel : OverlayVisualTextV3ViewModelBase
    {
        public override string DefaultHTML { get { return OverlayPollV3Model.DefaultHTML; } }
        public override string DefaultCSS { get { return OverlayPollV3Model.DefaultCSS; } }
        public override string DefaultJavascript { get { return OverlayPollV3Model.DefaultJavascript; } }

        public OverlayPollHeaderV3ViewModel Header
        {
            get { return this.header; }
            set
            {
                this.header = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayPollHeaderV3ViewModel header;

        public bool UseWithTwitchPolls
        {
            get { return this.useWithTwitchPolls; }
            set
            {
                this.useWithTwitchPolls = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool useWithTwitchPolls;
        public bool UseWithTwitchPredictions
        {
            get { return this.useWithTwitchPredictions; }
            set
            {
                this.useWithTwitchPredictions = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool useWithTwitchPredictions;
        public bool UseWithTriviaGameCommand
        {
            get { return this.useWithTriviaGameCommand; }
            set
            {
                this.useWithTriviaGameCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool useWithTriviaGameCommand;  
        public bool UseWithBetGameCommand
        {
            get { return this.useWithBetGameCommand; }
            set
            {
                this.useWithBetGameCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool useWithBetGameCommand;

        public string BackgroundColor
        {
            get { return this.backgroundColor; }
            set
            {
                this.backgroundColor = value;
                this.NotifyPropertyChanged();
            }
        }
        private string backgroundColor;
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
        public string BarColor
        {
            get { return this.barColor; }
            set
            {
                this.barColor = value;
                this.NotifyPropertyChanged();
            }
        }
        private string barColor;

        public bool UseTwitchPredictionColors
        {
            get { return this.useTwitchPredictionColor; }
            set
            {
                this.useTwitchPredictionColor = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool useTwitchPredictionColor;

        public OverlayAnimationV3ViewModel EntranceAnimation;
        public OverlayAnimationV3ViewModel ExitAnimation;

        public OverlayPollV3ViewModel()
            : base(OverlayItemV3Type.Poll)
        {
            this.Header = new OverlayPollHeaderV3ViewModel();

            this.Width = "400";

            this.BackgroundColor = "DarkGreen";
            this.BorderColor = "Black";
            this.BarColor = "Red";

            this.EntranceAnimation = new OverlayAnimationV3ViewModel(Resources.Entrance, new OverlayAnimationV3Model());
            this.ExitAnimation = new OverlayAnimationV3ViewModel(Resources.Exit, new OverlayAnimationV3Model());

            this.Initialize();
        }

        public OverlayPollV3ViewModel(OverlayPollV3Model item)
            : base(item)
        {
            this.Header = new OverlayPollHeaderV3ViewModel(item.Header);

            this.width = item.Width;

            this.UseWithTwitchPolls = item.UseWithTwitchPolls;
            this.UseWithTwitchPredictions = item.UseWithTwitchPredictions;
            this.UseWithBetGameCommand = item.UseWithBetGameCommand;
            this.UseWithTriviaGameCommand = item.UseWithTriviaGameCommand;

            this.BackgroundColor = item.BackgroundColor;
            this.BorderColor = item.BorderColor;
            this.BarColor = item.BarColor;

            this.UseTwitchPredictionColors = item.UseTwitchPredictionColors;

            this.EntranceAnimation = new OverlayAnimationV3ViewModel(Resources.Entrance, item.EntranceAnimation);
            this.ExitAnimation = new OverlayAnimationV3ViewModel(Resources.Exit, item.ExitAnimation);

            this.Initialize();
        }

        public override Result Validate()
        {
            return new Result();
        }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            OverlayPollV3Model result = new OverlayPollV3Model()
            {
                Width = this.width,

                UseWithTwitchPolls = this.UseWithTwitchPolls,
                UseWithTwitchPredictions = this.UseWithTwitchPredictions,
                UseWithBetGameCommand = this.UseWithBetGameCommand,
                UseWithTriviaGameCommand = this.UseWithTriviaGameCommand,

                BackgroundColor = this.BackgroundColor,
                BorderColor = this.BorderColor,
                BarColor = this.BarColor,

                UseTwitchPredictionColors = this.UseTwitchPredictionColors
            };

            this.AssignProperties(result);

            result.Header = (OverlayPollHeaderV3Model)this.header.GetItem();

            result.EntranceAnimation = this.EntranceAnimation.GetAnimation();
            result.ExitAnimation = this.ExitAnimation.GetAnimation();

            return result;
        }

        private void Initialize()
        {
            this.Animations.Add(this.EntranceAnimation);
            this.Animations.Add(this.ExitAnimation);

            this.Header.PropertyChanged += (sender, e) =>
            {
                this.NotifyPropertyChanged("X");
            };
        }
    }
}