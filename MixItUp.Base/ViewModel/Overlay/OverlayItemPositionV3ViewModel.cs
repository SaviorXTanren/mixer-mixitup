using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModels;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Overlay
{
    public enum OverlayItemPositionV3Type
    {
        Percentage,
        Pixel,
    }

    public class OverlayItemPositionV3ViewModel : UIViewModelBase
    {
        public OverlayItemPositionV3Type PositionType
        {
            get { return this.positionType; }
            set
            {
                this.positionType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(IsSimplePosition));
                this.NotifyPropertyChanged(nameof(IsPercentagePosition));
                this.NotifyPropertyChanged(nameof(IsPixelPosition));
                this.NotifyPropertyChanged(nameof(IsTopLeftSimplePosition));
                this.NotifyPropertyChanged(nameof(IsTopMiddleSimplePosition));
                this.NotifyPropertyChanged(nameof(IsTopRightSimplePosition));
                this.NotifyPropertyChanged(nameof(IsMiddleLeftSimplePosition));
                this.NotifyPropertyChanged(nameof(IsMiddleMiddleSimplePosition));
                this.NotifyPropertyChanged(nameof(IsMiddleRightSimplePosition));
                this.NotifyPropertyChanged(nameof(IsBottomLeftSimplePosition));
                this.NotifyPropertyChanged(nameof(IsBottomMiddleSimplePosition));
                this.NotifyPropertyChanged(nameof(IsBottomRightSimplePosition));
            }
        }
        private OverlayItemPositionV3Type positionType;

        public bool IsSimplePosition
        {
            get
            {
                return this.positionType == OverlayItemPositionV3Type.Percentage && this.Layer == 0 &&
                    (this.Horizontal == 25 || this.Horizontal == 50 || this.Horizontal == 75) &&
                    (this.Vertical == 25 || this.Vertical == 50 || this.Vertical == 75);
            }
        }
        public bool IsPercentagePosition { get { return !this.IsSimplePosition && this.positionType == OverlayItemPositionV3Type.Percentage; } }
        public bool IsPixelPosition { get { return this.positionType == OverlayItemPositionV3Type.Pixel; } }

        public bool IsTopLeftSimplePosition { get { return (this.positionType == OverlayItemPositionV3Type.Percentage && this.Horizontal == 25 && this.Vertical == 25); } }
        public bool IsTopMiddleSimplePosition { get { return (this.positionType == OverlayItemPositionV3Type.Percentage && this.Horizontal == 50 && this.Vertical == 25); } }
        public bool IsTopRightSimplePosition { get { return (this.positionType == OverlayItemPositionV3Type.Percentage && this.Horizontal == 75 && this.Vertical == 25); } }
        public bool IsMiddleLeftSimplePosition { get { return (this.positionType == OverlayItemPositionV3Type.Percentage && this.Horizontal == 25 && this.Vertical == 50); } }
        public bool IsMiddleMiddleSimplePosition { get { return (this.positionType == OverlayItemPositionV3Type.Percentage && this.Horizontal == 50 && this.Vertical == 50); } }
        public bool IsMiddleRightSimplePosition { get { return (this.positionType == OverlayItemPositionV3Type.Percentage && this.Horizontal == 75 && this.Vertical == 50); } }
        public bool IsBottomLeftSimplePosition { get { return (this.positionType == OverlayItemPositionV3Type.Percentage && this.Horizontal == 25 && this.Vertical == 75); } }
        public bool IsBottomMiddleSimplePosition { get { return (this.positionType == OverlayItemPositionV3Type.Percentage && this.Horizontal == 50 && this.Vertical == 75); } }
        public bool IsBottomRightSimplePosition { get { return (this.positionType == OverlayItemPositionV3Type.Percentage && this.Horizontal == 75 && this.Vertical == 75); } }

        public int Horizontal
        {
            get { return this.horizontal; }
            set
            {
                this.horizontal = value;
                this.NotifyPropertyChanged();
            }
        }
        private int horizontal;

        public int Vertical
        {
            get { return this.vertical; }
            set
            {
                this.vertical = value;
                this.NotifyPropertyChanged();
            }
        }
        private int vertical;

        public int Layer
        {
            get { return this.layer; }
            set
            {
                this.layer = value;
                this.NotifyPropertyChanged();
            }
        }
        private int layer;

        public string Duration
        {
            get { return this.duration; }
            set
            {
                this.duration = value;
                this.NotifyPropertyChanged();
            }
        }
        private string duration;

        public ICommand SimplePositionCommand { get; set; }
        public ICommand PercentagePositionCommand { get; set; }
        public ICommand PixelPositionCommand { get; set; }

        public ICommand TopLeftSimplePositionCommand { get; set; }
        public ICommand TopMiddleSimplePositionCommand { get; set; }
        public ICommand TopRightSimplePositionCommand { get; set; }
        public ICommand MiddleLeftSimplePositionCommand { get; set; }
        public ICommand MiddleMiddleSimplePositionCommand { get; set; }
        public ICommand MiddleRightSimplePositionCommand { get; set; }
        public ICommand BottomLeftSimplePositionCommand { get; set; }
        public ICommand BottomMiddleSimplePositionCommand { get; set; }
        public ICommand BottomRightSimplePositionCommand { get; set; }

        public OverlayItemPositionV3ViewModel()
        {
            this.SetCommands();

            this.PositionType = OverlayItemPositionV3Type.Percentage;
            this.Horizontal = 50;
            this.Vertical = 50;
            this.Duration = "5";
        }

        public OverlayItemPositionV3ViewModel(OverlayItemV3ModelBase position)
            : this()
        {
            this.SetCommands();

            this.PositionType = position.IsPercentagePosition ? OverlayItemPositionV3Type.Percentage : OverlayItemPositionV3Type.Pixel;
            this.Horizontal = position.XPosition;
            this.Vertical = position.YPosition;
            this.Layer = position.Layer;
            this.Duration = position.Duration;
        }

        public void SetPosition(OverlayItemV3ModelBase position)
        {
            position.IsPercentagePosition = this.PositionType == OverlayItemPositionV3Type.Percentage;
            position.XPosition = this.Horizontal;
            position.YPosition = this.Vertical;
            position.Layer = this.Layer;
            position.Duration = this.Duration;

            if (position.IsPercentagePosition)
            {
                position.XTranslation = -50;
                position.YTranslation = -50;
            }
        }

        private void SetCommands()
        {
            this.SimplePositionCommand = this.CreateCommand(() =>
            {
                this.Horizontal = 50;
                this.Vertical = 50;
                this.PositionType = OverlayItemPositionV3Type.Percentage;
            });
            this.PercentagePositionCommand = this.CreateCommand(() =>
            {
                this.Horizontal = 0;
                this.Vertical = 0;
                this.PositionType = OverlayItemPositionV3Type.Percentage;
            });
            this.PixelPositionCommand = this.CreateCommand(() =>
            {
                this.Horizontal = 0;
                this.Vertical = 0;
                this.PositionType = OverlayItemPositionV3Type.Pixel;
            });

            this.TopLeftSimplePositionCommand = this.CreateCommand(() => { this.AssignSimplePosition(25, 25); });
            this.TopMiddleSimplePositionCommand = this.CreateCommand(() => { this.AssignSimplePosition(50, 25); });
            this.TopRightSimplePositionCommand = this.CreateCommand(() => { this.AssignSimplePosition(75, 25); });
            this.MiddleLeftSimplePositionCommand = this.CreateCommand(() => { this.AssignSimplePosition(25, 50); });
            this.MiddleMiddleSimplePositionCommand = this.CreateCommand(() => { this.AssignSimplePosition(50, 50); });
            this.MiddleRightSimplePositionCommand = this.CreateCommand(() => { this.AssignSimplePosition(75, 50); });
            this.BottomLeftSimplePositionCommand = this.CreateCommand(() => { this.AssignSimplePosition(25, 75); });
            this.BottomMiddleSimplePositionCommand = this.CreateCommand(() => { this.AssignSimplePosition(50, 75); });
            this.BottomRightSimplePositionCommand = this.CreateCommand(() => { this.AssignSimplePosition(75, 75); });
        }

        private void AssignSimplePosition(int horizontal, int vertical)
        {
            this.Horizontal = horizontal;
            this.Vertical = vertical;
            this.PositionType = OverlayItemPositionV3Type.Percentage;
            this.Layer = 0;
        }
    }
}
