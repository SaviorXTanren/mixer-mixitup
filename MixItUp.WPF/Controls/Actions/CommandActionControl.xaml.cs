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
    public partial class CommandActionControl : ActionControlBase
    {
        private static readonly List<int> sampleFontSize = new List<int>() { 12, 24, 36, 48, 60, 72, 84, 96, 108, 120 };

        private CommandAction action;

        public CommandActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public CommandActionControl(ActionContainerControl containerControl, CommandAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            this.TypeComboBox.ItemsSource = EnumHelper.GetEnumNames<CommandActionTypeEnum>();

            //if (this.action != null)
            //{
            //    if (this.action.Effect is OverlayImageEffect)
            //    {
            //        OverlayImageEffect imageEffect = (OverlayImageEffect)this.action.Effect;
            //        this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayEffectTypeEnum.Image);
            //        this.ImageFilePathTextBox.Text = imageEffect.FilePath;
            //        this.ImageWidthTextBox.Text = imageEffect.Width.ToString();
            //        this.ImageHeightTextBox.Text = imageEffect.Height.ToString();
            //    }
            //    else if (this.action.Effect is OverlayTextEffect)
            //    {
            //        OverlayTextEffect textEffect = (OverlayTextEffect)this.action.Effect;
            //        this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayEffectTypeEnum.Text);
            //        this.TextTextBox.Text = textEffect.Text;
            //        this.FontSizeComboBox.Text = textEffect.Size.ToString();
            //        string color = textEffect.Color;
            //        if (ColorSchemes.ColorSchemeDictionary.ContainsValue(color))
            //        {
            //            color = ColorSchemes.ColorSchemeDictionary.FirstOrDefault(c => c.Value.Equals(color)).Key;
            //        }
            //        this.FontColorComboBox.Text = color;
            //    }
            //    else if (this.action.Effect is OverlayYoutubeEffect)
            //    {
            //        OverlayYoutubeEffect youtubeEffect = (OverlayYoutubeEffect)this.action.Effect;
            //        this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayEffectTypeEnum.YouTube);
            //        this.YoutubeVideoIDTextBox.Text = youtubeEffect.ID;
            //        this.YoutubeStartTimeTextBox.Text = youtubeEffect.StartTime.ToString();
            //        this.YoutubeWidthTextBox.Text = youtubeEffect.Width.ToString();
            //        this.YoutubeHeightTextBox.Text = youtubeEffect.Height.ToString();
            //    }
            //    else if (this.action.Effect is OverlayVideoEffect)
            //    {
            //        OverlayVideoEffect videoEffect = (OverlayVideoEffect)this.action.Effect;
            //        this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayEffectTypeEnum.Video);
            //        this.VideoFilePathTextBox.Text = videoEffect.FilePath;
            //        this.VideoWidthTextBox.Text = videoEffect.Width.ToString();
            //        this.VideoHeightTextBox.Text = videoEffect.Height.ToString();
            //    }
            //    else if (this.action.Effect is OverlayWebPageEffect)
            //    {
            //        OverlayWebPageEffect webPageEffect = (OverlayWebPageEffect)this.action.Effect;
            //        this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayEffectTypeEnum.WebPage);
            //        this.WebPageFilePathTextBox.Text = webPageEffect.URL;
            //        this.WebPageWidthTextBox.Text = webPageEffect.Width.ToString();
            //        this.WebPageHeightTextBox.Text = webPageEffect.Height.ToString();
            //    }
            //    else if (this.action.Effect is OverlayHTMLEffect)
            //    {
            //        OverlayHTMLEffect htmlEffect = (OverlayHTMLEffect)this.action.Effect;
            //        this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayEffectTypeEnum.HTML);
            //        this.HTMLTextBox.Text = htmlEffect.HTMLText;
            //    }

            //    this.HorizontalSlider.Value = this.action.Effect.Horizontal;
            //    this.VerticalSlider.Value = this.action.Effect.Vertical;

            //    if (this.action.Effect.Horizontal == 25 && this.action.Effect.Vertical == 25) { this.TopLeftPositionButton_Click(this, new RoutedEventArgs()); }
            //    else if (this.action.Effect.Horizontal == 50 && this.action.Effect.Vertical == 25) { this.TopPositionButton_Click(this, new RoutedEventArgs()); }
            //    else if (this.action.Effect.Horizontal == 75 && this.action.Effect.Vertical == 25) { this.TopRightPositionButton_Click(this, new RoutedEventArgs()); }
            //    else if (this.action.Effect.Horizontal == 25 && this.action.Effect.Vertical == 50) { this.LeftPositionButton_Click(this, new RoutedEventArgs()); }
            //    else if (this.action.Effect.Horizontal == 50 && this.action.Effect.Vertical == 50) { this.CenterPositionButton_Click(this, new RoutedEventArgs()); }
            //    else if (this.action.Effect.Horizontal == 75 && this.action.Effect.Vertical == 50) { this.RightPositionButton_Click(this, new RoutedEventArgs()); }
            //    else if (this.action.Effect.Horizontal == 25 && this.action.Effect.Vertical == 75) { this.BottomLeftPositionButton_Click(this, new RoutedEventArgs()); }
            //    else if (this.action.Effect.Horizontal == 50 && this.action.Effect.Vertical == 75) { this.BottomPositionButton_Click(this, new RoutedEventArgs()); }
            //    else if (this.action.Effect.Horizontal == 75 && this.action.Effect.Vertical == 75) { this.BottomRightPositionButton_Click(this, new RoutedEventArgs()); }
            //    else
            //    {
            //        this.PositionSimpleAdvancedToggleButton.IsChecked = true;
            //    }

            //    this.DurationTextBox.Text = this.action.Effect.Duration.ToString();
            //    this.EntranceAnimationComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.Effect.EntranceAnimation);
            //    this.VisibleAnimationComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.Effect.VisibleAnimation);
            //    this.ExitAnimationComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.Effect.ExitAnimation);
            //}
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            CommandActionTypeEnum type = EnumHelper.GetEnumValueFromString<CommandActionTypeEnum>((string)this.TypeComboBox.SelectedItem);
            switch (type)
            {
                case CommandActionTypeEnum.RunCommand:
                    break;
                case CommandActionTypeEnum.EnableDisableCommand:
                    break;
            }

            return null;
        }

        private void CommandTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // SETUP OPTIONS
        }
    }
}
