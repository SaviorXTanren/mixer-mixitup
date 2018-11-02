using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Model.Overlay;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Actions
{
    public enum OverlayActionTypeEnum
    {
        Text,
        Image,
        Video,
        YouTube,
        HTML,
        [Name("Web Page")]
        WebPage
    }

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
            if (ChannelSession.Settings.EnableOverlay)
            {
                this.OverlayNameComboBox.Visibility = Visibility.Visible;
                if (ChannelSession.Services.OverlayServers.GetOverlayNames().Count() > 1)
                {
                    this.OverlayNameComboBox.IsEnabled = true;
                    this.OverlayNameComboBox.ItemsSource = ChannelSession.Services.OverlayServers.GetOverlayNames();
                }
                else
                {
                    this.OverlayNameComboBox.IsEnabled = false;
                    this.OverlayNameComboBox.ItemsSource = new List<string>() { ChannelSession.Services.OverlayServers.DefaultOverlayName };
                    this.OverlayNameComboBox.SelectedIndex = 0;
                }
            }
            else
            {
                this.OverlayNotEnabledWarningTextBlock.Visibility = Visibility.Visible;
            }

            this.TypeComboBox.ItemsSource = EnumHelper.GetEnumNames<OverlayActionTypeEnum>();

            this.DurationTextBox.Text = "0";
            this.EntranceAnimationComboBox.ItemsSource = EnumHelper.GetEnumNames<OverlayEffectEntranceAnimationTypeEnum>();
            this.EntranceAnimationComboBox.SelectedIndex = 0;
            this.VisibleAnimationComboBox.ItemsSource = EnumHelper.GetEnumNames<OverlayEffectVisibleAnimationTypeEnum>();
            this.VisibleAnimationComboBox.SelectedIndex = 0;
            this.ExitAnimationComboBox.ItemsSource = EnumHelper.GetEnumNames<OverlayEffectExitAnimationTypeEnum>();
            this.ExitAnimationComboBox.SelectedIndex = 0;

            if (this.action != null)
            {
                this.DurationTextBox.Text = this.action.Effects.Duration.ToString();
                this.EntranceAnimationComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.Effects.EntranceAnimation);
                this.VisibleAnimationComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.Effects.VisibleAnimation);
                this.ExitAnimationComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.Effects.ExitAnimation);

                this.ItemPosition.SetPosition(this.action.Position);
               
                if (this.action.Item is OverlayImageItem)
                {
                    this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayActionTypeEnum.Image);
                    this.ImageItem.SetItem(this.action.Item);
                }
                else if (this.action.Item is OverlayTextItem)
                {
                    this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayActionTypeEnum.Text);
                    this.TextItem.SetItem(this.action.Item);
                }
                else if (this.action.Item is OverlayYouTubeItem)
                {
                    this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayActionTypeEnum.YouTube);
                    this.YouTubeItem.SetItem(this.action.Item);
                }
                else if (this.action.Item is OverlayVideoItem)
                {
                    this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayActionTypeEnum.Video);
                    this.VideoItem.SetItem(this.action.Item);
                }
                else if (this.action.Item is OverlayWebPageItem)
                {
                    this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayActionTypeEnum.WebPage);
                    this.WebPageItem.SetItem(this.action.Item);
                }
                else if (this.action.Item is OverlayHTMLItem)
                {
                    this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayActionTypeEnum.HTML);
                    this.HTMLItem.SetItem(this.action.Item);
                }
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (this.OverlayNameComboBox.SelectedIndex < 0)
            {
                return null;
            }
            string overlayName = (string)this.OverlayNameComboBox.SelectedItem;

            double duration;
            if (double.TryParse(this.DurationTextBox.Text, out duration) && duration > 0 && this.EntranceAnimationComboBox.SelectedIndex >= 0 &&
                this.VisibleAnimationComboBox.SelectedIndex >= 0 && this.ExitAnimationComboBox.SelectedIndex >= 0)
            {
                OverlayEffectEntranceAnimationTypeEnum entrance = EnumHelper.GetEnumValueFromString<OverlayEffectEntranceAnimationTypeEnum>((string)this.EntranceAnimationComboBox.SelectedItem);
                OverlayEffectVisibleAnimationTypeEnum visible = EnumHelper.GetEnumValueFromString<OverlayEffectVisibleAnimationTypeEnum>((string)this.VisibleAnimationComboBox.SelectedItem);
                OverlayEffectExitAnimationTypeEnum exit = EnumHelper.GetEnumValueFromString<OverlayEffectExitAnimationTypeEnum>((string)this.ExitAnimationComboBox.SelectedItem);

                OverlayItemEffects effect = new OverlayItemEffects(entrance, visible, exit, duration);

                OverlayItemPosition position = this.ItemPosition.GetPosition();

                OverlayItemBase item = null;

                OverlayActionTypeEnum overlayType = EnumHelper.GetEnumValueFromString<OverlayActionTypeEnum>((string)this.TypeComboBox.SelectedItem);
                if (overlayType == OverlayActionTypeEnum.Image)
                {
                    item = this.ImageItem.GetItem();
                }
                else if (overlayType == OverlayActionTypeEnum.Text)
                {
                    item = this.TextItem.GetItem();
                }
                else if (overlayType == OverlayActionTypeEnum.YouTube)
                {
                    item = this.YouTubeItem.GetItem();
                }
                else if (overlayType == OverlayActionTypeEnum.Video)
                {
                    item = this.VideoItem.GetItem();
                }
                else if (overlayType == OverlayActionTypeEnum.WebPage)
                {
                    item = this.WebPageItem.GetItem();
                }
                else if (overlayType == OverlayActionTypeEnum.HTML)
                {
                    item = this.HTMLItem.GetItem();
                }

                if (item != null)
                {
                    return new OverlayAction(overlayName, item, position, effect);
                }
            }
            return null;
        }

        private void OverlayTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.ImageItem.Visibility = Visibility.Collapsed;
            this.TextItem.Visibility = Visibility.Collapsed;
            this.YouTubeItem.Visibility = Visibility.Collapsed;
            this.VideoItem.Visibility = Visibility.Collapsed;
            this.WebPageItem.Visibility = Visibility.Collapsed;
            this.HTMLItem.Visibility = Visibility.Collapsed;
            this.AdditionalOptionsGrid.Visibility = Visibility.Collapsed;
            if (this.TypeComboBox.SelectedIndex >= 0)
            {
                OverlayActionTypeEnum overlayType = EnumHelper.GetEnumValueFromString<OverlayActionTypeEnum>((string)this.TypeComboBox.SelectedItem);
                if (overlayType == OverlayActionTypeEnum.Image)
                {
                    this.ImageItem.Visibility = Visibility.Visible;
                }
                else if (overlayType == OverlayActionTypeEnum.Text)
                {
                    this.TextItem.Visibility = Visibility.Visible;
                }
                else if (overlayType == OverlayActionTypeEnum.YouTube)
                {
                    this.YouTubeItem.Visibility = Visibility.Visible;
                }
                else if (overlayType == OverlayActionTypeEnum.Video)
                {
                    this.VideoItem.Visibility = Visibility.Visible;
                }
                else if (overlayType == OverlayActionTypeEnum.WebPage)
                {
                    this.WebPageItem.Visibility = Visibility.Visible;
                }
                else if (overlayType == OverlayActionTypeEnum.HTML)
                {
                    this.HTMLItem.Visibility = Visibility.Visible;
                }
                this.AdditionalOptionsGrid.Visibility = Visibility.Visible;
            }
        }
    }
}
