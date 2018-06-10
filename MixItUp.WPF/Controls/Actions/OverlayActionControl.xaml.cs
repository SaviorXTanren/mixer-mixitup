using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Themes;
using System.Collections.Generic;
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
        private static readonly List<int> sampleFontSize = new List<int>() { 12, 24, 36, 48, 60, 72, 84, 96, 108, 120 };

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
            this.FontSizeComboBox.ItemsSource = OverlayActionControl.sampleFontSize.Select(f => f.ToString());
            this.FontColorComboBox.ItemsSource = ColorSchemes.ColorSchemeDictionary.Keys;
            this.YoutubeStartTimeTextBox.Text = "0";
            this.YoutubeWidthTextBox.Text = this.VideoWidthTextBox.Text = OverlayVideoEffect.DefaultWidth.ToString();
            this.YoutubeHeightTextBox.Text = this.VideoHeightTextBox.Text = OverlayVideoEffect.DefaultHeight.ToString();

            this.CenterPositionButton_Click(this, new RoutedEventArgs());

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
                    this.FontSizeComboBox.Text = textEffect.Size.ToString();
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

                this.HorizontalSlider.Value = this.action.Effect.Horizontal;
                this.VerticalSlider.Value = this.action.Effect.Vertical;

                if (this.action.Effect.Horizontal == 25 && this.action.Effect.Vertical == 25) { this.TopLeftPositionButton_Click(this, new RoutedEventArgs()); }
                else if (this.action.Effect.Horizontal == 50 && this.action.Effect.Vertical == 25) { this.TopPositionButton_Click(this, new RoutedEventArgs()); }
                else if (this.action.Effect.Horizontal == 75 && this.action.Effect.Vertical == 25) { this.TopRightPositionButton_Click(this, new RoutedEventArgs()); }
                else if (this.action.Effect.Horizontal == 25 && this.action.Effect.Vertical == 50) { this.LeftPositionButton_Click(this, new RoutedEventArgs()); }
                else if (this.action.Effect.Horizontal == 50 && this.action.Effect.Vertical == 50) { this.CenterPositionButton_Click(this, new RoutedEventArgs()); }
                else if (this.action.Effect.Horizontal == 75 && this.action.Effect.Vertical == 50) { this.RightPositionButton_Click(this, new RoutedEventArgs()); }
                else if (this.action.Effect.Horizontal == 25 && this.action.Effect.Vertical == 75) { this.BottomLeftPositionButton_Click(this, new RoutedEventArgs()); }
                else if (this.action.Effect.Horizontal == 50 && this.action.Effect.Vertical == 75) { this.BottomPositionButton_Click(this, new RoutedEventArgs()); }
                else if (this.action.Effect.Horizontal == 75 && this.action.Effect.Vertical == 75) { this.BottomRightPositionButton_Click(this, new RoutedEventArgs()); }
                else
                {
                    this.PositionSimpleAdvancedToggleButton.IsChecked = true;
                }

                this.DurationTextBox.Text = this.action.Effect.Duration.ToString();
                this.EntranceAnimationComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.Effect.EntranceAnimation);
                this.VisibleAnimationComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.Effect.VisibleAnimation);
                this.ExitAnimationComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.Effect.ExitAnimation);
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

                int horizontal = 0;
                int vertical = 0;
                if (this.PositionSimpleAdvancedToggleButton.IsChecked.GetValueOrDefault())
                {
                    horizontal = (int)this.HorizontalSlider.Value;
                    vertical = (int)this.VerticalSlider.Value;
                }
                else
                {
                    if (this.IsSimplePositionButtonSelected(this.TopLeftPositionButton))
                    {
                        horizontal = 25;
                        vertical = 25;
                    }
                    else if (this.IsSimplePositionButtonSelected(this.TopPositionButton))
                    {
                        horizontal = 50;
                        vertical = 25;
                    }
                    else if (this.IsSimplePositionButtonSelected(this.TopRightPositionButton))
                    {
                        horizontal = 75;
                        vertical = 25;
                    }
                    else if (this.IsSimplePositionButtonSelected(this.LeftPositionButton))
                    {
                        horizontal = 25;
                        vertical = 50;
                    }
                    else if (this.IsSimplePositionButtonSelected(this.CenterPositionButton))
                    {
                        horizontal = 50;
                        vertical = 50;
                    }
                    else if (this.IsSimplePositionButtonSelected(this.RightPositionButton))
                    {
                        horizontal = 75;
                        vertical = 50;
                    }
                    else if (this.IsSimplePositionButtonSelected(this.BottomLeftPositionButton))
                    {
                        horizontal = 25;
                        vertical = 75;
                    }
                    else if (this.IsSimplePositionButtonSelected(this.BottomPositionButton))
                    {
                        horizontal = 50;
                        vertical = 75;
                    }
                    else if (this.IsSimplePositionButtonSelected(this.BottomRightPositionButton))
                    {
                        horizontal = 75;
                        vertical = 75;
                    }
                }

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

                        if (int.TryParse(this.FontSizeComboBox.Text, out int size) && size > 0)
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
            this.AdditionalOptionsGrid.Visibility = Visibility.Collapsed;
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
                this.AdditionalOptionsGrid.Visibility = Visibility.Visible;
            }
        }

        private void ImageFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog(ChannelSession.Services.FileService.ImageFileFilter());
            if (!string.IsNullOrEmpty(filePath))
            {
                this.ImageFilePathTextBox.Text = filePath;
            }
        }

        private void VideoFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog(ChannelSession.Services.FileService.VideoFileFilter());
            if (!string.IsNullOrEmpty(filePath))
            {
                this.VideoFilePathTextBox.Text = filePath;
            }
        }

        private void WebPageFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog(ChannelSession.Services.FileService.HTMLFileFilter());
            if (!string.IsNullOrEmpty(filePath))
            {
                this.WebPageFilePathTextBox.Text = filePath;
            }
        }

        private void PositionSimpleAdvancedToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            this.SimplePositionGrid.Visibility = (this.PositionSimpleAdvancedToggleButton.IsChecked.GetValueOrDefault()) ? Visibility.Hidden : Visibility.Visible;
            this.AdvancedPositionGrid.Visibility = (this.PositionSimpleAdvancedToggleButton.IsChecked.GetValueOrDefault()) ? Visibility.Visible : Visibility.Hidden;
        }

        private void TopLeftPositionButton_Click(object sender, RoutedEventArgs e) { this.HandleSimplePositionChange(this.TopLeftPositionButton); }

        private void TopPositionButton_Click(object sender, RoutedEventArgs e) { this.HandleSimplePositionChange(this.TopPositionButton); }

        private void TopRightPositionButton_Click(object sender, RoutedEventArgs e) { this.HandleSimplePositionChange(this.TopRightPositionButton); }

        private void LeftPositionButton_Click(object sender, RoutedEventArgs e) { this.HandleSimplePositionChange(this.LeftPositionButton); }

        private void CenterPositionButton_Click(object sender, RoutedEventArgs e) { this.HandleSimplePositionChange(this.CenterPositionButton); }

        private void RightPositionButton_Click(object sender, RoutedEventArgs e) { this.HandleSimplePositionChange(this.RightPositionButton); }

        private void BottomLeftPositionButton_Click(object sender, RoutedEventArgs e) { this.HandleSimplePositionChange(this.BottomLeftPositionButton); }

        private void BottomPositionButton_Click(object sender, RoutedEventArgs e) { this.HandleSimplePositionChange(this.BottomPositionButton); }

        private void BottomRightPositionButton_Click(object sender, RoutedEventArgs e) { this.HandleSimplePositionChange(this.BottomRightPositionButton); }

        private void HandleSimplePositionChange(Button button)
        {
            this.TopLeftPositionButton.Style = (Style)this.FindResource("MaterialDesignRaisedButton");
            this.TopPositionButton.Style = (Style)this.FindResource("MaterialDesignRaisedButton");
            this.TopRightPositionButton.Style = (Style)this.FindResource("MaterialDesignRaisedButton");
            this.LeftPositionButton.Style = (Style)this.FindResource("MaterialDesignRaisedButton");
            this.CenterPositionButton.Style = (Style)this.FindResource("MaterialDesignRaisedButton");
            this.RightPositionButton.Style = (Style)this.FindResource("MaterialDesignRaisedButton");
            this.BottomLeftPositionButton.Style = (Style)this.FindResource("MaterialDesignRaisedButton");
            this.BottomPositionButton.Style = (Style)this.FindResource("MaterialDesignRaisedButton");
            this.BottomRightPositionButton.Style = (Style)this.FindResource("MaterialDesignRaisedButton");

            button.Style = (Style)this.FindResource("MaterialDesignRaisedLightButton");
        }

        private bool IsSimplePositionButtonSelected(Button button) { return button.Style.Equals((Style)this.FindResource("MaterialDesignRaisedLightButton")); }
    }
}
