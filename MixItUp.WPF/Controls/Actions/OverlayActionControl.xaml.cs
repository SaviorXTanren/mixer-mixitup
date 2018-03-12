using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for ChatActionControl.xaml
    /// </summary>
    public partial class OverlayActionControl : ActionControlBase
    {
        private enum OverlayTypeEnum
        {
            Image,
            Text,
            Youtube,
            HTML,
            Video,
        }

        private enum OverlayFadeEffectEnum
        {
            Instant = 0,
            Fast = 200,
            Medium = 400,
            Slow = 600,
        }

        private OverlayAction action;

        public OverlayActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public OverlayActionControl(ActionContainerControl containerControl, OverlayAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            this.OverlayTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<OverlayTypeEnum>();
            this.OverlayFadeEffectComboBox.ItemsSource = EnumHelper.GetEnumNames<OverlayFadeEffectEnum>();
            this.OverlayYoutubeStartTimeTextBox.Text = "0";
            this.OverlayYoutubeWidthTextBox.Text = this.OverlayVideoWidthTextBox.Text = "560";
            this.OverlayYoutubeHeightTextBox.Text = this.OverlayVideoHeightTextBox.Text = "316";

            if (this.action != null)
            {
                if (!string.IsNullOrEmpty(this.action.ImagePath))
                {
                    this.OverlayTypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayTypeEnum.Image);
                    this.OverlayImageFilePathTextBox.Text = this.action.ImagePath;
                    this.OverlayImageWidthTextBox.Text = this.action.ImageWidth.ToString();
                    this.OverlayImageHeightTextBox.Text = this.action.ImageHeight.ToString();
                }
                else if (!string.IsNullOrEmpty(this.action.Text))
                {
                    this.OverlayTypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayTypeEnum.Text);
                    this.OverlayTextTextBox.Text = this.action.Text;
                    this.OverlayFontSizeTextBox.Text = this.action.FontSize.ToString();
                    this.OverlayFontColorTextBox.Text = this.action.Color;
                }
                else if (!string.IsNullOrEmpty(this.action.youtubeVideoID))
                {
                    this.OverlayTypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayTypeEnum.Youtube);
                    this.OverlayYoutubeVideoIDTextBox.Text = this.action.youtubeVideoID;
                    this.OverlayYoutubeStartTimeTextBox.Text = this.action.youtubeStartTime.ToString();
                    this.OverlayYoutubeWidthTextBox.Text = this.action.VideoWidth.ToString();
                    this.OverlayYoutubeHeightTextBox.Text = this.action.VideoHeight.ToString();
                }
                else if (!string.IsNullOrEmpty(this.action.localVideoFilePath))
                {
                    this.OverlayTypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayTypeEnum.Video);
                    this.OverlayVideoFilePathTextBox.Text = this.action.localVideoFilePath;
                    this.OverlayVideoWidthTextBox.Text = this.action.VideoWidth.ToString();
                    this.OverlayVideoHeightTextBox.Text = this.action.VideoHeight.ToString();
                }
                else if (!string.IsNullOrEmpty(this.action.HTMLText))
                {
                    this.OverlayTypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayTypeEnum.HTML);
                    this.OverlayHTMLTextBox.Text = this.action.HTMLText;
                }
                this.OverlayDurationTextBox.Text = this.action.Duration.ToString();
                this.OverlayHorizontalSlider.Value = this.action.Horizontal;
                this.OverlayVerticalSlider.Value = this.action.Vertical;
                this.OverlayFadeEffectComboBox.SelectedItem = ((OverlayFadeEffectEnum)this.action.FadeDuration).ToString();
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            double duration;
            if (double.TryParse(this.OverlayDurationTextBox.Text, out duration) && duration > 0 && this.OverlayFadeEffectComboBox.SelectedIndex >= 0)
            {
                int horizontal = (int)this.OverlayHorizontalSlider.Value;
                int vertical = (int)this.OverlayVerticalSlider.Value;
                int fadeDuration = (int)EnumHelper.GetEnumValueFromString<OverlayFadeEffectEnum>((string)this.OverlayFadeEffectComboBox.SelectedItem);

                OverlayTypeEnum type = EnumHelper.GetEnumValueFromString<OverlayTypeEnum>((string)this.OverlayTypeComboBox.SelectedItem);
                if (type == OverlayTypeEnum.Image)
                {
                    if (!string.IsNullOrEmpty(this.OverlayImageFilePathTextBox.Text))
                    {
                        int width;
                        int height;
                        if (int.TryParse(this.OverlayImageWidthTextBox.Text, out width) && width > 0 &&
                            int.TryParse(this.OverlayImageHeightTextBox.Text, out height) && height > 0)
                        {
                            return OverlayAction.CreateForImage(this.OverlayImageFilePathTextBox.Text, width, height, duration, horizontal, vertical, fadeDuration);
                        }
                    }
                }
                else if (type == OverlayTypeEnum.Text)
                {
                    if (!string.IsNullOrEmpty(this.OverlayTextTextBox.Text) && !string.IsNullOrEmpty(this.OverlayFontColorTextBox.Text))
                    {
                        int fontSize;
                        if (int.TryParse(this.OverlayFontSizeTextBox.Text, out fontSize) && fontSize > 0)
                        {
                            return OverlayAction.CreateForText(this.OverlayTextTextBox.Text, this.OverlayFontColorTextBox.Text, fontSize, duration, horizontal, vertical, fadeDuration);
                        }
                    }
                }
                else if (type == OverlayTypeEnum.Youtube)
                {
                    if (!string.IsNullOrEmpty(this.OverlayYoutubeVideoIDTextBox.Text))
                    {
                        string videoID = this.OverlayYoutubeVideoIDTextBox.Text;
                        videoID = videoID.Replace("https://www.youtube.com/watch?v=", "");
                        videoID = videoID.Replace("https://youtu.be/", "");
                        if (videoID.Contains("&"))
                        {
                            videoID = videoID.Substring(0, videoID.IndexOf("&"));
                        }

                        if (int.TryParse(this.OverlayYoutubeStartTimeTextBox.Text, out int startTime))
                        {
                            int width;
                            int height;
                            if (int.TryParse(this.OverlayYoutubeWidthTextBox.Text, out width) && width > 0 &&
                                int.TryParse(this.OverlayYoutubeHeightTextBox.Text, out height) && height > 0)
                            {
                                return OverlayAction.CreateForYoutube(videoID, startTime, width, height, duration, horizontal, vertical, fadeDuration);
                            }
                        }
                    }
                }
                else if (type == OverlayTypeEnum.Video)
                {
                    if (!string.IsNullOrEmpty(this.OverlayVideoFilePathTextBox.Text))
                    {
                        int width;
                        int height;
                        if (int.TryParse(this.OverlayVideoWidthTextBox.Text, out width) && width > 0 &&
                            int.TryParse(this.OverlayVideoHeightTextBox.Text, out height) && height > 0)
                        {
                            return OverlayAction.CreateForVideo(this.OverlayVideoFilePathTextBox.Text, width, height, duration, horizontal, vertical, fadeDuration);
                        }
                    }
                }
                else if (type == OverlayTypeEnum.HTML)
                {
                    if (!string.IsNullOrEmpty(this.OverlayHTMLTextBox.Text))
                    {
                        return OverlayAction.CreateForHTML(this.OverlayHTMLTextBox.Text, duration, horizontal, vertical, fadeDuration); ;
                    }
                }
            }
            return null;
        }

        private void OverlayTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.OverlayPositionGrid.Visibility = Visibility.Collapsed;
            this.OverlayImageGrid.Visibility = Visibility.Collapsed;
            this.OverlayTextGrid.Visibility = Visibility.Collapsed;
            this.OverlayYoutubeVideoGrid.Visibility = Visibility.Collapsed;
            this.OverlayVideoGrid.Visibility = Visibility.Collapsed;
            this.OverlayHTMLGrid.Visibility = Visibility.Collapsed;
            if (this.OverlayTypeComboBox.SelectedIndex >= 0)
            {
                OverlayTypeEnum overlayType = EnumHelper.GetEnumValueFromString<OverlayTypeEnum>((string)this.OverlayTypeComboBox.SelectedItem);
                if (overlayType == OverlayTypeEnum.Image)
                {
                    this.OverlayImageGrid.Visibility = Visibility.Visible;
                }
                else if (overlayType == OverlayTypeEnum.Text)
                {
                    this.OverlayTextGrid.Visibility = Visibility.Visible;
                }
                else if (overlayType == OverlayTypeEnum.Youtube)
                {
                    this.OverlayYoutubeVideoGrid.Visibility = Visibility.Visible;
                }
                else if (overlayType == OverlayTypeEnum.Video)
                {
                    this.OverlayVideoGrid.Visibility = Visibility.Visible;
                }
                else if (overlayType == OverlayTypeEnum.HTML)
                {
                    this.OverlayHTMLGrid.Visibility = Visibility.Visible;
                }
                this.OverlayPositionGrid.Visibility = Visibility.Visible;
            }
        }

        private void OverlayImageFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog();
            if (!string.IsNullOrEmpty(filePath))
            {
                this.OverlayImageFilePathTextBox.Text = filePath;
            }
        }

        private void OverlayVideoFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog("MP4 Files (*.mp4)|*.mp4|WEBM Files (*.webm)|*.webm|All files (*.*)|*.*");
            if (!string.IsNullOrEmpty(filePath))
            {
                this.OverlayVideoFilePathTextBox.Text = filePath;
            }
        }
    }
}
