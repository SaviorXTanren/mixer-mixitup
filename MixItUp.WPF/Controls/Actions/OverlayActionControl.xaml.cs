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
            HTML,
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
            if (this.action != null)
            {
                if (!string.IsNullOrEmpty(this.action.ImagePath) || !string.IsNullOrEmpty(this.action.Text) || !string.IsNullOrEmpty(this.action.HTMLText))
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
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            double duration;
            if (!string.IsNullOrEmpty(this.OverlayImageFilePathTextBox.Text) || !(string.IsNullOrEmpty(this.OverlayTextTextBox.Text)) || !string.IsNullOrEmpty(this.OverlayHTMLTextBox.Text))
            {
                if (double.TryParse(this.OverlayDurationTextBox.Text, out duration) && duration > 0 && this.OverlayFadeEffectComboBox.SelectedIndex >= 0)
                {
                    int horizontal = (int)this.OverlayHorizontalSlider.Value;
                    int vertical = (int)this.OverlayVerticalSlider.Value;
                    int fadeDuration = (int)EnumHelper.GetEnumValueFromString<OverlayFadeEffectEnum>((string)this.OverlayFadeEffectComboBox.SelectedItem);
                    if (!string.IsNullOrEmpty(this.OverlayImageFilePathTextBox.Text))
                    {
                        int width;
                        int height;
                        if (int.TryParse(this.OverlayImageWidthTextBox.Text, out width) && width > 0 &&
                            int.TryParse(this.OverlayImageHeightTextBox.Text, out height) && height > 0)
                        {
                            return new OverlayAction(this.OverlayImageFilePathTextBox.Text, width, height, duration, horizontal, vertical, fadeDuration);
                        }
                    }
                    else if (!string.IsNullOrEmpty(this.OverlayTextTextBox.Text) && !string.IsNullOrEmpty(this.OverlayFontColorTextBox.Text))
                    {
                        int fontSize;
                        if (int.TryParse(this.OverlayFontSizeTextBox.Text, out fontSize) && fontSize > 0)
                        {
                            return new OverlayAction(this.OverlayTextTextBox.Text, this.OverlayFontColorTextBox.Text, fontSize, duration, horizontal, vertical, fadeDuration);
                        }
                    }
                    else if (!string.IsNullOrEmpty(this.OverlayHTMLTextBox.Text))
                    {
                        return new OverlayAction(this.OverlayHTMLTextBox.Text, duration, horizontal, vertical, fadeDuration);
                    }
                }
            }
            return null;
        }

        private void OverlayTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.OverlayPositionGrid.Visibility = Visibility.Hidden;
            this.OverlayImageGrid.Visibility = Visibility.Hidden;
            this.OverlayTextGrid.Visibility = Visibility.Hidden;
            this.OverlayHTMLGrid.Visibility = Visibility.Hidden;
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
    }
}
