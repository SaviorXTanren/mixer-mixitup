using MixItUp.Base.Model.Overlay;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Overlay
{
    [Obsolete]
    public class OverlayItemPositionViewModel : ControlViewModelBase
    {
        public OverlayItemPositionType PositionType
        {
            get { return this.positionType; }
            set
            {
                this.positionType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("IsSimplePosition");
                this.NotifyPropertyChanged("IsPercentagePosition");
                this.NotifyPropertyChanged("IsPixelPosition");
                this.NotifyPropertyChanged("IsTopLeftSimplePosition");
                this.NotifyPropertyChanged("IsTopMiddleSimplePosition");
                this.NotifyPropertyChanged("IsTopRightSimplePosition");
                this.NotifyPropertyChanged("IsMiddleLeftSimplePosition");
                this.NotifyPropertyChanged("IsMiddleMiddleSimplePosition");
                this.NotifyPropertyChanged("IsMiddleRightSimplePosition");
                this.NotifyPropertyChanged("IsBottomLeftSimplePosition");
                this.NotifyPropertyChanged("IsBottomMiddleSimplePosition");
                this.NotifyPropertyChanged("IsBottomRightSimplePosition");
            }
        }
        private OverlayItemPositionType positionType;

        public bool IsSimplePosition
        {
            get
            {
                return this.positionType == OverlayItemPositionType.Percentage && this.Layer == 0 &&
                    (this.Horizontal == 25 || this.Horizontal == 50 || this.Horizontal == 75) &&
                    (this.Vertical == 25 || this.Vertical == 50 || this.Vertical == 75);
            }
        }
        public bool IsPercentagePosition { get { return !this.IsSimplePosition && this.positionType == OverlayItemPositionType.Percentage; } }
        public bool IsPixelPosition { get { return this.positionType == OverlayItemPositionType.Pixel; } }

        public bool IsTopLeftSimplePosition { get { return (this.positionType == OverlayItemPositionType.Percentage && this.Horizontal == 25 && this.Vertical == 25); } }
        public bool IsTopMiddleSimplePosition { get { return (this.positionType == OverlayItemPositionType.Percentage && this.Horizontal == 50 && this.Vertical == 25); } }
        public bool IsTopRightSimplePosition { get { return (this.positionType == OverlayItemPositionType.Percentage && this.Horizontal == 75 && this.Vertical == 25); } }
        public bool IsMiddleLeftSimplePosition { get { return (this.positionType == OverlayItemPositionType.Percentage && this.Horizontal == 25 && this.Vertical == 50); } }
        public bool IsMiddleMiddleSimplePosition { get { return (this.positionType == OverlayItemPositionType.Percentage && this.Horizontal == 50 && this.Vertical == 50); } }
        public bool IsMiddleRightSimplePosition { get { return (this.positionType == OverlayItemPositionType.Percentage && this.Horizontal == 75 && this.Vertical == 50); } }
        public bool IsBottomLeftSimplePosition { get { return (this.positionType == OverlayItemPositionType.Percentage && this.Horizontal == 25 && this.Vertical == 75); } }
        public bool IsBottomMiddleSimplePosition { get { return (this.positionType == OverlayItemPositionType.Percentage && this.Horizontal == 50 && this.Vertical == 75); } }
        public bool IsBottomRightSimplePosition { get { return (this.positionType == OverlayItemPositionType.Percentage && this.Horizontal == 75 && this.Vertical == 75); } }

        public string HorizontalString
        {
            get { return this.horizontal.ToString(); }
            set
            {
                this.horizontal = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged("HorizontalString");
                this.NotifyPropertyChanged("Horizontal");
            }
        }
        public int Horizontal
        {
            get { return this.horizontal; }
            set
            {
                this.horizontal = value;
                this.NotifyPropertyChanged("HorizontalString");
                this.NotifyPropertyChanged("Horizontal");
            }
        }
        private int horizontal;

        public string VerticalString
        {
            get { return this.vertical.ToString(); }
            set
            {
                this.vertical = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged("VerticalString");
                this.NotifyPropertyChanged("Vertical");
            }
        }
        public int Vertical
        {
            get { return this.vertical; }
            set
            {
                this.vertical = value;
                this.NotifyPropertyChanged("VerticalString");
                this.NotifyPropertyChanged("Vertical");
            }
        }
        private int vertical;

        public string LayerString
        {
            get { return this.layer.ToString(); }
            set
            {
                this.layer = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged("LayerString");
                this.NotifyPropertyChanged("Layer");
            }
        }
        public int Layer
        {
            get { return this.layer; }
            set
            {
                this.layer = value;
                this.NotifyPropertyChanged("LayerString");
                this.NotifyPropertyChanged("Layer");
            }
        }
        private int layer;

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

        public OverlayItemPositionViewModel() : this(new OverlayItemPositionModel(OverlayItemPositionType.Percentage, 50, 50, 0)) { }

        public OverlayItemPositionViewModel(OverlayItemPositionModel position)
        {
            this.SimplePositionCommand = this.CreateCommand(() =>
            {
                this.Horizontal = 50;
                this.Vertical = 50;
                this.PositionType = OverlayItemPositionType.Percentage;
            });
            this.PercentagePositionCommand = this.CreateCommand(() =>
            {
                this.Horizontal = 0;
                this.Vertical = 0;
                this.PositionType = OverlayItemPositionType.Percentage;
            });
            this.PixelPositionCommand = this.CreateCommand(() =>
            {
                this.Horizontal = 0;
                this.Vertical = 0;
                this.PositionType = OverlayItemPositionType.Pixel;
            });

            this.TopLeftSimplePositionCommand = this.CreateCommand(() => { this.SetSimplePosition(25, 25); });
            this.TopMiddleSimplePositionCommand = this.CreateCommand(() => { this.SetSimplePosition(50, 25); });
            this.TopRightSimplePositionCommand = this.CreateCommand(() => { this.SetSimplePosition(75, 25); });
            this.MiddleLeftSimplePositionCommand = this.CreateCommand(() => { this.SetSimplePosition(25, 50); });
            this.MiddleMiddleSimplePositionCommand = this.CreateCommand(() => { this.SetSimplePosition(50, 50); });
            this.MiddleRightSimplePositionCommand = this.CreateCommand(() => { this.SetSimplePosition(75, 50); });
            this.BottomLeftSimplePositionCommand = this.CreateCommand(() => { this.SetSimplePosition(25, 75); });
            this.BottomMiddleSimplePositionCommand = this.CreateCommand(() => { this.SetSimplePosition(50, 75); });
            this.BottomRightSimplePositionCommand = this.CreateCommand(() => { this.SetSimplePosition(75, 75); });

            this.SetPosition(position);
        }

        public void SetPosition(OverlayItemPositionModel position)
        {
            this.PositionType = position.PositionType;
            this.Horizontal = position.Horizontal;
            this.Vertical = position.Vertical;
            this.Layer = position.Layer;
        }

        public OverlayItemPositionModel GetPosition()
        {
            return new OverlayItemPositionModel(this.PositionType, this.Horizontal, this.Vertical, this.layer);
        }

        private void SetSimplePosition(int horizontal, int vertical)
        {
            this.Horizontal = horizontal;
            this.Vertical = vertical;
            this.PositionType = OverlayItemPositionType.Percentage;
            this.Layer = 0;
        }
    }
}
