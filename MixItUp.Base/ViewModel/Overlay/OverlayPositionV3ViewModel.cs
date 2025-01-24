using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayPositionV3ViewModel : UIViewModelBase
    {
        public event EventHandler PositionUpdated = delegate { };

        public IEnumerable<OverlayPositionV3Type> PositionTypes { get; } = EnumHelper.GetEnumList<OverlayPositionV3Type>();
        public OverlayPositionV3Type SelectedPositionType
        {
            get { return this.selectedPositionType; }
            set
            {
                this.selectedPositionType = value;

                this.NotifyPropertyChanged();

                this.NotifyPropertyChanged(nameof(IsSimplePosition));
                this.NotifyPropertyChanged(nameof(IsPercentagePosition));
                this.NotifyPropertyChanged(nameof(IsPixelPosition));
                this.NotifyPropertyChanged(nameof(IsRandomPosition));

                if (this.SelectedPositionType == OverlayPositionV3Type.Simple)
                {
                    this.XPosition = 50;
                    this.YPosition = 50;
                    this.NotifySimplePositionButtons();
                }
                else if (this.SelectedPositionType == OverlayPositionV3Type.Percentage)
                {
                    this.XPosition = 50;
                    this.YPosition = 50;
                }
                else if (this.SelectedPositionType == OverlayPositionV3Type.Pixel || this.SelectedPositionType == OverlayPositionV3Type.Random)
                {
                    this.XPosition = 0;
                    this.YPosition = 0;
                }
            }
        }
        private OverlayPositionV3Type selectedPositionType;

        public bool IsSimplePosition { get { return this.SelectedPositionType == OverlayPositionV3Type.Simple; } }
        public bool IsPercentagePosition { get { return this.SelectedPositionType == OverlayPositionV3Type.Percentage; } }
        public bool IsPixelPosition { get { return this.SelectedPositionType == OverlayPositionV3Type.Pixel; } }
        public bool IsRandomPosition { get { return this.SelectedPositionType == OverlayPositionV3Type.Random; } }

        public bool IsTopLeftSimplePosition { get { return this.XPosition == 25 && this.YPosition == 25; } }
        public bool IsTopMiddleSimplePosition { get { return this.XPosition == 50 && this.YPosition == 25; } }
        public bool IsTopRightSimplePosition { get { return this.XPosition == 75 && this.YPosition == 25; } }
        public bool IsMiddleLeftSimplePosition { get { return this.XPosition == 25 && this.YPosition == 50; } }
        public bool IsMiddleMiddleSimplePosition { get { return this.XPosition == 50 && this.YPosition == 50; } }
        public bool IsMiddleRightSimplePosition { get { return this.XPosition == 75 && this.YPosition == 50; } }
        public bool IsBottomLeftSimplePosition { get { return this.XPosition == 25 && this.YPosition == 75; } }
        public bool IsBottomMiddleSimplePosition { get { return this.XPosition == 50 && this.YPosition == 75; } }
        public bool IsBottomRightSimplePosition { get { return this.XPosition == 75 && this.YPosition == 75; } }

        public int XPosition
        {
            get { return this.xPosition; }
            set
            {
                this.xPosition = value;
                this.NotifyPropertyChanged();

                if (this.SelectedPositionType == OverlayPositionV3Type.Simple)
                {
                    this.NotifySimplePositionButtons();
                }
            }
        }
        private int xPosition;

        public int YPosition
        {
            get { return this.yPosition; }
            set
            {
                this.yPosition = value;
                this.NotifyPropertyChanged();

                if (this.SelectedPositionType == OverlayPositionV3Type.Simple)
                {
                    this.NotifySimplePositionButtons();
                }
            }
        }
        private int yPosition;

        public int XMaximumPosition
        {
            get { return this.xMaximumPosition; }
            set
            {
                this.xMaximumPosition = value;
                this.NotifyPropertyChanged();
            }
        }
        private int xMaximumPosition = 1920;

        public int YMaximumPosition
        {
            get { return this.yMaximumPosition; }
            set
            {
                this.yMaximumPosition = value;
                this.NotifyPropertyChanged();
            }
        }
        private int yMaximumPosition = 1080;

        public int Layer
        {
            get { return this.layer; }
            set
            {
                this.layer = value > 0 ? value : 0;
                this.NotifyPropertyChanged();
            }
        }
        private int layer;

        private DelayedCountSemaphore delayedCountSemaphore = new DelayedCountSemaphore(delay: 200);

        public ICommand TopLeftSimplePositionCommand { get; set; }
        public ICommand TopMiddleSimplePositionCommand { get; set; }
        public ICommand TopRightSimplePositionCommand { get; set; }
        public ICommand MiddleLeftSimplePositionCommand { get; set; }
        public ICommand MiddleMiddleSimplePositionCommand { get; set; }
        public ICommand MiddleRightSimplePositionCommand { get; set; }
        public ICommand BottomLeftSimplePositionCommand { get; set; }
        public ICommand BottomMiddleSimplePositionCommand { get; set; }
        public ICommand BottomRightSimplePositionCommand { get; set; }

        public OverlayPositionV3ViewModel()
        {
            this.SelectedPositionType = OverlayPositionV3Type.Simple;

            this.Initialize();
        }

        public OverlayPositionV3ViewModel(OverlayItemV3ModelBase item)
        {
            this.SelectedPositionType = item.PositionType;
            if (this.SelectedPositionType == OverlayPositionV3Type.Random)
            {
                this.XPosition = item.XPosition;
                this.YPosition = item.YPosition;
                this.XMaximumPosition = item.XMaximum;
                this.YMaximumPosition = item.YMaximum;
            }
            else
            {
                this.XPosition = item.XPosition;
                this.YPosition = item.YPosition;
            }
            this.Layer = item.Layer;

            this.Initialize();
        }

        public void SetPosition(OverlayItemV3ModelBase item)
        {
            item.PositionType = this.SelectedPositionType;
            item.XPosition = this.XPosition;
            item.YPosition = this.YPosition;
            item.Layer = this.Layer;
            if (this.SelectedPositionType == OverlayPositionV3Type.Random)
            {
                item.XMaximum = this.XMaximumPosition;
                item.YMaximum = this.YMaximumPosition;
            }
        }

        public Result Validate()
        {
            if (this.IsPercentagePosition)
            {
                if (this.XPosition < 0 || this.XPosition > 100 || this.YPosition < 0 || this.YPosition > 100)
                {
                    return new Result(Resources.OverlayPositionPercentageBetween0And100);
                }
            }
            else if (this.IsRandomPosition)
            {
                if (this.XPosition > this.XMaximumPosition || this.YPosition > this.YMaximumPosition)
                {
                    return new Result(Resources.OverlayPositionRandomMinimumsCantBeGreaterThanMaximums);
                }
            }
            return new Result();
        }

        private void Initialize()
        {
            this.PropertyChanged += OverlayPositionV3ViewModel_PropertyChanged;

            this.delayedCountSemaphore.Completed += DelayedCountSemaphore_Completed;

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
            this.SelectedPositionType = OverlayPositionV3Type.Simple;
            this.XPosition = horizontal;
            this.YPosition = vertical;
            this.Layer = 0;
        }

        private void NotifySimplePositionButtons()
        {
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

        private void OverlayPositionV3ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.delayedCountSemaphore.Add();
        }

        private void DelayedCountSemaphore_Completed(object sender, EventArgs e)
        {
            this.PositionUpdated(this, new EventArgs());
        }
    }
}
