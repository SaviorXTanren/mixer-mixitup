using MixItUp.Base.Commands;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using System.Linq;

namespace MixItUp.Base.ViewModel.Controls.Overlay
{
    public class OverlayTimerItemViewModel : OverlayCustomHTMLItemViewModelBase
    {
        public string TotalLengthString
        {
            get { return this.totalLength.ToString(); }
            set
            {
                this.totalLength = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        private int totalLength;

        public string SizeString
        {
            get { return this.size.ToString(); }
            set
            {
                this.size = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        private int size;

        public string Font
        {
            get { return this.font; }
            set
            {
                this.font = value;
                this.NotifyPropertyChanged();
            }
        }
        private string font;

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

        public CustomCommand TimerCompleteCommand
        {
            get { return this.timerCompleteCommand; }
            set
            {
                this.timerCompleteCommand = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("IsTimerCompleteCommandSet");
                this.NotifyPropertyChanged("IsTimerCompleteCommandNotSet");
            }
        }
        private CustomCommand timerCompleteCommand;

        public bool IsTimerCompleteCommandSet { get { return this.TimerCompleteCommand != null; } }
        public bool IsTimerCompleteCommandNotSet { get { return !this.IsTimerCompleteCommandSet; } }

        public OverlayTimerItemViewModel()
        {
            this.Font = "Arial";
            this.HTML = OverlayTimer.HTMLTemplate;
        }

        public OverlayTimerItemViewModel(OverlayTimer item)
            : this()
        {
            this.totalLength = item.TotalLength;
            this.size = item.TextSize;
            this.Font = item.TextFont;

            this.Color = item.TextColor;
            if (ColorSchemes.HTMLColorSchemeDictionary.ContainsValue(this.Color))
            {
                this.Color = ColorSchemes.HTMLColorSchemeDictionary.FirstOrDefault(c => c.Value.Equals(this.Color)).Key;
            }

            this.HTML = item.HTMLText;

            this.TimerCompleteCommand = item.TimerCompleteCommand;
        }

        public override OverlayItemBase GetItem()
        {
            if (!string.IsNullOrEmpty(this.Font) && !string.IsNullOrEmpty(this.Color) && !string.IsNullOrEmpty(this.HTML) && this.size > 0 && this.totalLength > 0)
            {
                if (ColorSchemes.HTMLColorSchemeDictionary.ContainsKey(this.Color))
                {
                    this.Color = ColorSchemes.HTMLColorSchemeDictionary[this.Color];
                }
                return new OverlayTimer(this.HTML, this.totalLength, this.Color, this.Font, size, this.TimerCompleteCommand);
            }
            return null;
        }
    }
}
