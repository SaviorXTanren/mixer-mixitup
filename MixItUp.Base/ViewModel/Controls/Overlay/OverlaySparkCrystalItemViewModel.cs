using Mixer.Base.Util;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Overlay
{
    public class OverlaySparkCrystalItemViewModel : OverlayHTMLTemplateItemViewModelBase
    {
        public string WidthString
        {
            get { return this.width.ToString(); }
            set
            {
                this.width = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        protected int width;

        public string HeightString
        {
            get { return this.height.ToString(); }
            set
            {
                this.height = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        protected int height;

        public string CustomImageFilePath
        {
            get { return this.customImageFilePath; }
            set
            {
                this.customImageFilePath = value;
                this.NotifyPropertyChanged();
            }
        }
        private string customImageFilePath;

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

        public string TextColor
        {
            get { return this.textColor; }
            set
            {
                this.textColor = value;
                this.NotifyPropertyChanged();
            }
        }
        private string textColor;

        public string ProgressAnimationString
        {
            get { return EnumHelper.GetEnumName(this.progressAnimation); }
            set
            {
                this.progressAnimation = EnumHelper.GetEnumValueFromString<OverlayItemEffectVisibleAnimationTypeEnum>(value);
                this.NotifyPropertyChanged();
            }
        }
        protected OverlayItemEffectVisibleAnimationTypeEnum progressAnimation;

        public string MilestoneReachedAnimationString
        {
            get { return EnumHelper.GetEnumName(this.milestoneReachedAnimation); }
            set
            {
                this.milestoneReachedAnimation = EnumHelper.GetEnumValueFromString<OverlayItemEffectVisibleAnimationTypeEnum>(value);
                this.NotifyPropertyChanged();
            }
        }
        protected OverlayItemEffectVisibleAnimationTypeEnum milestoneReachedAnimation;

        public ICommand BrowseFilePathCommand { get; set; }

        public OverlaySparkCrystalItemViewModel()
        {
            this.BrowseFilePathCommand = this.CreateCommand((parameter) =>
            {
                string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog(ChannelSession.Services.FileService.ImageFileFilter());
                if (!string.IsNullOrEmpty(filePath))
                {
                    this.CustomImageFilePath = filePath;
                }
                return Task.FromResult(0);
            });

            this.width = 130;
            this.height = 400;
            this.Font = "Arial";
            this.TextColor = "Black";

            this.HTML = OverlaySparkCrystalItemModel.HTMLTemplate;
        }

        public OverlaySparkCrystalItemViewModel(OverlaySparkCrystalItemModel item)
            : this()
        {
            this.width = item.CrystalWidth;
            this.height = item.CrystalHeight;
            this.CustomImageFilePath = item.CustomImageFilePath;

            this.Font = item.TextFont;
            this.TextColor = ColorSchemes.GetColorName(item.TextColor);

            this.progressAnimation = item.ProgressAnimation;
            this.milestoneReachedAnimation = item.MilestoneReachedAnimation;

            this.HTML = item.HTML;
        }

        public override OverlayItemModelBase GetOverlayItem()
        {
            if (this.width > 0 && this.height > 0 && !string.IsNullOrEmpty(this.Font) && !string.IsNullOrEmpty(this.TextColor) && !string.IsNullOrEmpty(this.HTML))
            {
                this.TextColor = ColorSchemes.GetColorCode(this.TextColor);

                return new OverlaySparkCrystalItemModel(this.HTML, this.TextColor, this.Font, this.width, this.height, this.CustomImageFilePath, this.progressAnimation, this.milestoneReachedAnimation);
            }
            return null;
        }
    }
}
