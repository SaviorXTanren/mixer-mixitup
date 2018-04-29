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
            this.OverlayTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<OverlayEffectTypeEnum>();
            this.OverlayFontColorComboBox.ItemsSource = ColorSchemes.ColorSchemeDictionary.Keys;
            this.OverlayYoutubeStartTimeTextBox.Text = "0";
            this.OverlayYoutubeWidthTextBox.Text = this.OverlayVideoWidthTextBox.Text = OverlayVideoEffect.DefaultWidth.ToString();
            this.OverlayYoutubeHeightTextBox.Text = this.OverlayVideoHeightTextBox.Text = OverlayVideoEffect.DefaultHeight.ToString();

            this.OverlayEntranceAnimationComboBox.ItemsSource = EnumHelper.GetEnumNames<OverlayEffectEntranceAnimationTypeEnum>();
            this.OverlayEntranceAnimationComboBox.SelectedIndex = 0;
            this.OverlayVisibleAnimationComboBox.ItemsSource = EnumHelper.GetEnumNames<OverlayEffectVisibleAnimationTypeEnum>();
            this.OverlayVisibleAnimationComboBox.SelectedIndex = 0;
            this.OverlayExitAnimationComboBox.ItemsSource = EnumHelper.GetEnumNames<OverlayEffectExitAnimationTypeEnum>();
            this.OverlayExitAnimationComboBox.SelectedIndex = 0;

            if (this.action != null)
            {
                if (this.action.Effect is OverlayImageEffect)
                {
                    OverlayImageEffect imageEffect = (OverlayImageEffect)this.action.Effect;
                    this.OverlayTypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayEffectTypeEnum.Image);
                    this.OverlayImageFilePathTextBox.Text = imageEffect.FilePath;
                    this.OverlayImageWidthTextBox.Text = imageEffect.Width.ToString();
                    this.OverlayImageHeightTextBox.Text = imageEffect.Height.ToString();
                }
                else if (this.action.Effect is OverlayTextEffect)
                {
                    OverlayTextEffect textEffect = (OverlayTextEffect)this.action.Effect;
                    this.OverlayTypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayEffectTypeEnum.Text);
                    this.OverlayTextTextBox.Text = textEffect.Text;
                    this.OverlayFontSizeTextBox.Text = textEffect.Size.ToString();
                    string color = textEffect.Color;
                    if (ColorSchemes.ColorSchemeDictionary.ContainsValue(color))
                    {
                        color = ColorSchemes.ColorSchemeDictionary.FirstOrDefault(c => c.Value.Equals(color)).Key;
                    }
                    this.OverlayFontColorComboBox.Text = color;
                }
                else if (this.action.Effect is OverlayYoutubeEffect)
                {
                    OverlayYoutubeEffect youtubeEffect = (OverlayYoutubeEffect)this.action.Effect;
                    this.OverlayTypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayEffectTypeEnum.Youtube);
                    this.OverlayYoutubeVideoIDTextBox.Text = youtubeEffect.ID;
                    this.OverlayYoutubeStartTimeTextBox.Text = youtubeEffect.StartTime.ToString();
                    this.OverlayYoutubeWidthTextBox.Text = youtubeEffect.Width.ToString();
                    this.OverlayYoutubeHeightTextBox.Text = youtubeEffect.Height.ToString();
                }
                else if (this.action.Effect is OverlayVideoEffect)
                {
                    OverlayVideoEffect videoEffect = (OverlayVideoEffect)this.action.Effect;
                    this.OverlayTypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayEffectTypeEnum.Video);
                    this.OverlayVideoFilePathTextBox.Text = videoEffect.FilePath;
                    this.OverlayVideoWidthTextBox.Text = videoEffect.Width.ToString();
                    this.OverlayVideoHeightTextBox.Text = videoEffect.Height.ToString();
                }
                else if (this.action.Effect is OverlayHTMLEffect)
                {
                    OverlayHTMLEffect htmlEffect = (OverlayHTMLEffect)this.action.Effect;
                    this.OverlayTypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayEffectTypeEnum.HTML);
                    this.OverlayHTMLTextBox.Text = htmlEffect.HTMLText;
                }
                this.OverlayDurationTextBox.Text = this.action.Effect.Duration.ToString();
                this.OverlayEntranceAnimationComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.Effect.EntranceAnimation);
                this.OverlayVisibleAnimationComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.Effect.VisibleAnimation);
                this.OverlayExitAnimationComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.Effect.ExitAnimation);
                this.OverlayHorizontalSlider.Value = this.action.Effect.Horizontal;
                this.OverlayVerticalSlider.Value = this.action.Effect.Vertical;
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            double duration;
            if (double.TryParse(this.OverlayDurationTextBox.Text, out duration) && duration > 0 && this.OverlayEntranceAnimationComboBox.SelectedIndex >= 0 &&
                this.OverlayVisibleAnimationComboBox.SelectedIndex >= 0 && this.OverlayExitAnimationComboBox.SelectedIndex >= 0)
            {
                OverlayEffectEntranceAnimationTypeEnum entrance = EnumHelper.GetEnumValueFromString<OverlayEffectEntranceAnimationTypeEnum>((string)this.OverlayEntranceAnimationComboBox.SelectedItem);
                OverlayEffectVisibleAnimationTypeEnum animation = EnumHelper.GetEnumValueFromString<OverlayEffectVisibleAnimationTypeEnum>((string)this.OverlayVisibleAnimationComboBox.SelectedItem);
                OverlayEffectExitAnimationTypeEnum exit = EnumHelper.GetEnumValueFromString<OverlayEffectExitAnimationTypeEnum>((string)this.OverlayExitAnimationComboBox.SelectedItem);
                int horizontal = (int)this.OverlayHorizontalSlider.Value;
                int vertical = (int)this.OverlayVerticalSlider.Value;

                OverlayEffectTypeEnum type = EnumHelper.GetEnumValueFromString<OverlayEffectTypeEnum>((string)this.OverlayTypeComboBox.SelectedItem);
                if (type == OverlayEffectTypeEnum.Image)
                {
                    if (!string.IsNullOrEmpty(this.OverlayImageFilePathTextBox.Text))
                    {
                        int width;
                        int height;
                        if (int.TryParse(this.OverlayImageWidthTextBox.Text, out width) && width > 0 &&
                            int.TryParse(this.OverlayImageHeightTextBox.Text, out height) && height > 0)
                        {
                            return new OverlayAction(new OverlayImageEffect(this.OverlayImageFilePathTextBox.Text, width, height, entrance, animation, exit, duration, horizontal, vertical));
                        }
                    }
                }
                else if (type == OverlayEffectTypeEnum.Text)
                {
                    if (!string.IsNullOrEmpty(this.OverlayTextTextBox.Text) && !string.IsNullOrEmpty(this.OverlayFontColorComboBox.Text))
                    {
                        string color = this.OverlayFontColorComboBox.Text;
                        if (ColorSchemes.ColorSchemeDictionary.ContainsKey(color))
                        {
                            color = ColorSchemes.ColorSchemeDictionary[color];
                        }

                        if (int.TryParse(this.OverlayFontSizeTextBox.Text, out int size) && size > 0)
                        {
                            return new OverlayAction(new OverlayTextEffect(this.OverlayTextTextBox.Text, color, size, entrance, animation, exit, duration, horizontal, vertical));
                        }
                    }
                }
                else if (type == OverlayEffectTypeEnum.Youtube)
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
                                return new OverlayAction(new OverlayYoutubeEffect(videoID, startTime, width, height, entrance, animation, exit, duration, horizontal, vertical));
                            }
                        }
                    }
                }
                else if (type == OverlayEffectTypeEnum.Video)
                {
                    if (!string.IsNullOrEmpty(this.OverlayVideoFilePathTextBox.Text))
                    {
                        int width;
                        int height;
                        if (int.TryParse(this.OverlayVideoWidthTextBox.Text, out width) && width > 0 &&
                            int.TryParse(this.OverlayVideoHeightTextBox.Text, out height) && height > 0)
                        {
                            return new OverlayAction(new OverlayVideoEffect(this.OverlayVideoFilePathTextBox.Text, width, height, entrance, animation, exit, duration, horizontal, vertical));
                        }
                    }
                }
                else if (type == OverlayEffectTypeEnum.HTML)
                {
                    if (!string.IsNullOrEmpty(this.OverlayHTMLTextBox.Text))
                    {
                        return new OverlayAction(new OverlayHTMLEffect(this.OverlayHTMLTextBox.Text, entrance, animation, exit, duration, horizontal, vertical));
                    }
                }
            }
            return null;
        }

        private void OverlayTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.OverlayImageGrid1.Visibility = Visibility.Collapsed;
            this.OverlayImageGrid2.Visibility = Visibility.Collapsed;
            this.OverlayTextGrid1.Visibility = Visibility.Collapsed;
            this.OverlayTextGrid2.Visibility = Visibility.Collapsed;
            this.OverlayYoutubeGrid1.Visibility = Visibility.Collapsed;
            this.OverlayYoutubeGrid2.Visibility = Visibility.Collapsed;
            this.OverlayVideoGrid1.Visibility = Visibility.Collapsed;
            this.OverlayVideoGrid2.Visibility = Visibility.Collapsed;
            this.OverlayHTMLGrid.Visibility = Visibility.Collapsed;
            this.OverlayDurationAndAnimationsGrid.Visibility = Visibility.Collapsed;
            this.OverlayPositionGrid.Visibility = Visibility.Collapsed;
            if (this.OverlayTypeComboBox.SelectedIndex >= 0)
            {
                OverlayEffectTypeEnum overlayType = EnumHelper.GetEnumValueFromString<OverlayEffectTypeEnum>((string)this.OverlayTypeComboBox.SelectedItem);
                if (overlayType == OverlayEffectTypeEnum.Image)
                {
                    this.OverlayImageGrid1.Visibility = Visibility.Visible;
                    this.OverlayImageGrid2.Visibility = Visibility.Visible;
                }
                else if (overlayType == OverlayEffectTypeEnum.Text)
                {
                    this.OverlayTextGrid1.Visibility = Visibility.Visible;
                    this.OverlayTextGrid2.Visibility = Visibility.Visible;
                }
                else if (overlayType == OverlayEffectTypeEnum.Youtube)
                {
                    this.OverlayYoutubeGrid1.Visibility = Visibility.Visible;
                    this.OverlayYoutubeGrid2.Visibility = Visibility.Visible;
                }
                else if (overlayType == OverlayEffectTypeEnum.Video)
                {
                    this.OverlayVideoGrid1.Visibility = Visibility.Visible;
                    this.OverlayVideoGrid2.Visibility = Visibility.Visible;
                }
                else if (overlayType == OverlayEffectTypeEnum.HTML)
                {
                    this.OverlayHTMLGrid.Visibility = Visibility.Visible;
                }
                this.OverlayDurationAndAnimationsGrid.Visibility = Visibility.Visible;
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
