using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Themes;
using System.Linq;
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
        private OverlayAction action;

        public OverlayActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public OverlayActionControl(ActionContainerControl containerControl, OverlayAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            if (ChannelSession.Services.OverlayServer == null)
            {
                this.OverlayNotEnabledWarningTextBlock.Visibility = Visibility.Visible;
            }

            this.TypeComboBox.ItemsSource = EnumHelper.GetEnumNames<OverlayEffectTypeEnum>();
            this.FontColorComboBox.ItemsSource = ColorSchemes.ColorSchemeDictionary.Keys;
            this.YoutubeStartTimeTextBox.Text = "0";
            this.YoutubeWidthTextBox.Text = this.VideoWidthTextBox.Text = OverlayVideoEffect.DefaultWidth.ToString();
            this.YoutubeHeightTextBox.Text = this.VideoHeightTextBox.Text = OverlayVideoEffect.DefaultHeight.ToString();

            this.EntranceAnimationComboBox.ItemsSource = EnumHelper.GetEnumNames<OverlayEffectEntranceAnimationTypeEnum>();
            this.EntranceAnimationComboBox.SelectedIndex = 0;
            this.VisibleAnimationComboBox.ItemsSource = EnumHelper.GetEnumNames<OverlayEffectVisibleAnimationTypeEnum>();
            this.VisibleAnimationComboBox.SelectedIndex = 0;
            this.ExitAnimationComboBox.ItemsSource = EnumHelper.GetEnumNames<OverlayEffectExitAnimationTypeEnum>();
            this.ExitAnimationComboBox.SelectedIndex = 0;

            if (this.action != null)
            {
                if (this.action.Effect is OverlayImageEffect)
                {
                    OverlayImageEffect imageEffect = (OverlayImageEffect)this.action.Effect;
                    this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayEffectTypeEnum.Image);
                    this.ImageFilePathTextBox.Text = imageEffect.FilePath;
                    this.ImageWidthTextBox.Text = imageEffect.Width.ToString();
                    this.ImageHeightTextBox.Text = imageEffect.Height.ToString();
                }
                else if (this.action.Effect is OverlayTextEffect)
                {
                    OverlayTextEffect textEffect = (OverlayTextEffect)this.action.Effect;
                    this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayEffectTypeEnum.Text);
                    this.TextTextBox.Text = textEffect.Text;
                    this.FontSizeTextBox.Text = textEffect.Size.ToString();
                    string color = textEffect.Color;
                    if (ColorSchemes.ColorSchemeDictionary.ContainsValue(color))
                    {
                        color = ColorSchemes.ColorSchemeDictionary.FirstOrDefault(c => c.Value.Equals(color)).Key;
                    }
                    this.FontColorComboBox.Text = color;
                }
                else if (this.action.Effect is OverlayYoutubeEffect)
                {
                    OverlayYoutubeEffect youtubeEffect = (OverlayYoutubeEffect)this.action.Effect;
                    this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayEffectTypeEnum.YouTube);
                    this.YoutubeVideoIDTextBox.Text = youtubeEffect.ID;
                    this.YoutubeStartTimeTextBox.Text = youtubeEffect.StartTime.ToString();
                    this.YoutubeWidthTextBox.Text = youtubeEffect.Width.ToString();
                    this.YoutubeHeightTextBox.Text = youtubeEffect.Height.ToString();
                }
                else if (this.action.Effect is OverlayVideoEffect)
                {
                    OverlayVideoEffect videoEffect = (OverlayVideoEffect)this.action.Effect;
                    this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayEffectTypeEnum.Video);
                    this.VideoFilePathTextBox.Text = videoEffect.FilePath;
                    this.VideoWidthTextBox.Text = videoEffect.Width.ToString();
                    this.VideoHeightTextBox.Text = videoEffect.Height.ToString();
                }
                else if (this.action.Effect is OverlayWebPageEffect)
                {
                    OverlayWebPageEffect webPageEffect = (OverlayWebPageEffect)this.action.Effect;
                    this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayEffectTypeEnum.WebPage);
                    this.WebPageFilePathTextBox.Text = webPageEffect.URL;
                    this.WebPageWidthTextBox.Text = webPageEffect.Width.ToString();
                    this.WebPageHeightTextBox.Text = webPageEffect.Height.ToString();
                }
                else if (this.action.Effect is OverlayHTMLEffect)
                {
                    OverlayHTMLEffect htmlEffect = (OverlayHTMLEffect)this.action.Effect;
                    this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayEffectTypeEnum.HTML);
                    this.HTMLTextBox.Text = htmlEffect.HTMLText;
                }
                this.DurationTextBox.Text = this.action.Effect.Duration.ToString();
                this.EntranceAnimationComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.Effect.EntranceAnimation);
                this.VisibleAnimationComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.Effect.VisibleAnimation);
                this.ExitAnimationComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.Effect.ExitAnimation);
                this.HorizontalSlider.Value = this.action.Effect.Horizontal;
                this.VerticalSlider.Value = this.action.Effect.Vertical;
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            double duration;
            if (double.TryParse(this.DurationTextBox.Text, out duration) && duration > 0 && this.EntranceAnimationComboBox.SelectedIndex >= 0 &&
                this.VisibleAnimationComboBox.SelectedIndex >= 0 && this.ExitAnimationComboBox.SelectedIndex >= 0)
            {
                OverlayEffectEntranceAnimationTypeEnum entrance = EnumHelper.GetEnumValueFromString<OverlayEffectEntranceAnimationTypeEnum>((string)this.EntranceAnimationComboBox.SelectedItem);
                OverlayEffectVisibleAnimationTypeEnum animation = EnumHelper.GetEnumValueFromString<OverlayEffectVisibleAnimationTypeEnum>((string)this.VisibleAnimationComboBox.SelectedItem);
                OverlayEffectExitAnimationTypeEnum exit = EnumHelper.GetEnumValueFromString<OverlayEffectExitAnimationTypeEnum>((string)this.ExitAnimationComboBox.SelectedItem);
                int horizontal = (int)this.HorizontalSlider.Value;
                int vertical = (int)this.VerticalSlider.Value;

                OverlayEffectTypeEnum type = EnumHelper.GetEnumValueFromString<OverlayEffectTypeEnum>((string)this.TypeComboBox.SelectedItem);
                if (type == OverlayEffectTypeEnum.Image)
                {
                    if (!string.IsNullOrEmpty(this.ImageFilePathTextBox.Text))
                    {
                        int width;
                        int height;
                        if (int.TryParse(this.ImageWidthTextBox.Text, out width) && width > 0 &&
                            int.TryParse(this.ImageHeightTextBox.Text, out height) && height > 0)
                        {
                            return new OverlayAction(new OverlayImageEffect(this.ImageFilePathTextBox.Text, width, height, entrance, animation, exit, duration, horizontal, vertical));
                        }
                    }
                }
                else if (type == OverlayEffectTypeEnum.Text)
                {
                    if (!string.IsNullOrEmpty(this.TextTextBox.Text) && !string.IsNullOrEmpty(this.FontColorComboBox.Text))
                    {
                        string color = this.FontColorComboBox.Text;
                        if (ColorSchemes.ColorSchemeDictionary.ContainsKey(color))
                        {
                            color = ColorSchemes.ColorSchemeDictionary[color];
                        }

                        if (int.TryParse(this.FontSizeTextBox.Text, out int size) && size > 0)
                        {
                            return new OverlayAction(new OverlayTextEffect(this.TextTextBox.Text, color, size, entrance, animation, exit, duration, horizontal, vertical));
                        }
                    }
                }
                else if (type == OverlayEffectTypeEnum.YouTube)
                {
                    if (!string.IsNullOrEmpty(this.YoutubeVideoIDTextBox.Text))
                    {
                        string videoID = this.YoutubeVideoIDTextBox.Text;
                        videoID = videoID.Replace("https://www.youtube.com/watch?v=", "");
                        videoID = videoID.Replace("https://youtu.be/", "");
                        if (videoID.Contains("&"))
                        {
                            videoID = videoID.Substring(0, videoID.IndexOf("&"));
                        }

                        if (int.TryParse(this.YoutubeStartTimeTextBox.Text, out int startTime))
                        {
                            int width;
                            int height;
                            if (int.TryParse(this.YoutubeWidthTextBox.Text, out width) && width > 0 &&
                                int.TryParse(this.YoutubeHeightTextBox.Text, out height) && height > 0)
                            {
                                return new OverlayAction(new OverlayYoutubeEffect(videoID, startTime, width, height, entrance, animation, exit, duration, horizontal, vertical));
                            }
                        }
                    }
                }
                else if (type == OverlayEffectTypeEnum.Video)
                {
                    if (!string.IsNullOrEmpty(this.VideoFilePathTextBox.Text))
                    {
                        int width;
                        int height;
                        if (int.TryParse(this.VideoWidthTextBox.Text, out width) && width > 0 &&
                            int.TryParse(this.VideoHeightTextBox.Text, out height) && height > 0)
                        {
                            return new OverlayAction(new OverlayVideoEffect(this.VideoFilePathTextBox.Text, width, height, entrance, animation, exit, duration, horizontal, vertical));
                        }
                    }
                }
                else if (type == OverlayEffectTypeEnum.WebPage)
                {
                    if (!string.IsNullOrEmpty(this.WebPageFilePathTextBox.Text))
                    {
                        int width;
                        int height;
                        if (int.TryParse(this.WebPageWidthTextBox.Text, out width) && width > 0 &&
                            int.TryParse(this.WebPageHeightTextBox.Text, out height) && height > 0)
                        {
                            return new OverlayAction(new OverlayWebPageEffect(this.WebPageFilePathTextBox.Text, width, height, entrance, animation, exit, duration, horizontal, vertical));
                        }
                    }
                }
                else if (type == OverlayEffectTypeEnum.HTML)
                {
                    if (!string.IsNullOrEmpty(this.HTMLTextBox.Text))
                    {
                        return new OverlayAction(new OverlayHTMLEffect(this.HTMLTextBox.Text, entrance, animation, exit, duration, horizontal, vertical));
                    }
                }
            }
            return null;
        }

        private void OverlayTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.ImageGrid.Visibility = Visibility.Collapsed;
            this.TextGrid.Visibility = Visibility.Collapsed;
            this.YouTubeGrid.Visibility = Visibility.Collapsed;
            this.VideoGrid.Visibility = Visibility.Collapsed;
            this.WebPageGrid.Visibility = Visibility.Collapsed;
            this.HTMLGrid.Visibility = Visibility.Collapsed;
            this.DurationAndAnimationsGrid.Visibility = Visibility.Collapsed;
            this.PositionGrid.Visibility = Visibility.Collapsed;
            if (this.TypeComboBox.SelectedIndex >= 0)
            {
                OverlayEffectTypeEnum overlayType = EnumHelper.GetEnumValueFromString<OverlayEffectTypeEnum>((string)this.TypeComboBox.SelectedItem);
                if (overlayType == OverlayEffectTypeEnum.Image)
                {
                    this.ImageGrid.Visibility = Visibility.Visible;
                }
                else if (overlayType == OverlayEffectTypeEnum.Text)
                {
                    this.TextGrid.Visibility = Visibility.Visible;
                }
                else if (overlayType == OverlayEffectTypeEnum.YouTube)
                {
                    this.YouTubeGrid.Visibility = Visibility.Visible;
                }
                else if (overlayType == OverlayEffectTypeEnum.Video)
                {
                    this.VideoGrid.Visibility = Visibility.Visible;
                }
                else if (overlayType == OverlayEffectTypeEnum.WebPage)
                {
                    this.WebPageGrid.Visibility = Visibility.Visible;
                }
                else if (overlayType == OverlayEffectTypeEnum.HTML)
                {
                    this.HTMLGrid.Visibility = Visibility.Visible;
                }
                this.DurationAndAnimationsGrid.Visibility = Visibility.Visible;
                this.PositionGrid.Visibility = Visibility.Visible;
            }
        }

        private void ImageFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog();
            if (!string.IsNullOrEmpty(filePath))
            {
                this.ImageFilePathTextBox.Text = filePath;
            }
        }

        private void VideoFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog("MP4 Files (*.mp4)|*.mp4|WEBM Files (*.webm)|*.webm|All files (*.*)|*.*");
            if (!string.IsNullOrEmpty(filePath))
            {
                this.VideoFilePathTextBox.Text = filePath;
            }
        }

        private void WebPageFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog();
            if (!string.IsNullOrEmpty(filePath))
            {
                this.WebPageFilePathTextBox.Text = filePath;
            }
        }
    }
}
