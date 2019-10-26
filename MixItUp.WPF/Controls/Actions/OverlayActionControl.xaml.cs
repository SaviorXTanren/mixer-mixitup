using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Overlay;
using StreamingClient.Base.Util;
using System;
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
        private const string ShowHideWidgetOption = "Show/Hide Widget";

        private OverlayAction action;

        public OverlayActionControl() : base() { InitializeComponent(); }

        public OverlayActionControl(OverlayAction action) : this() { this.action = action; }

        public override Task OnLoaded()
        {
            if (ChannelSession.Settings.EnableOverlay)
            {
                if (ChannelSession.Services.OverlayServers.GetOverlayNames().Count() > 1)
                {
                    this.OverlayNameComboBox.IsEnabled = true;
                    this.OverlayNameComboBox.ItemsSource = ChannelSession.Services.OverlayServers.GetOverlayNames();
                }
                else
                {
                    this.OverlayNameComboBox.IsEnabled = false;
                    this.OverlayNameComboBox.ItemsSource = new List<string>() { ChannelSession.Services.OverlayServers.DefaultOverlayName };
                }
                this.OverlayNameComboBox.SelectedItem = ChannelSession.Services.OverlayServers.DefaultOverlayName;
            }
            else
            {
                this.OverlayNotEnabledWarningTextBlock.Visibility = Visibility.Visible;
            }

            List<string> typeOptions = new List<string>(EnumHelper.GetEnumNames<OverlayItemModelTypeEnum>(new List<OverlayItemModelTypeEnum>()
            {
                OverlayItemModelTypeEnum.Text, OverlayItemModelTypeEnum.Image, OverlayItemModelTypeEnum.Video,
                OverlayItemModelTypeEnum.YouTube, OverlayItemModelTypeEnum.WebPage, OverlayItemModelTypeEnum.HTML
            }));
            typeOptions.Add(ShowHideWidgetOption);

            this.TypeComboBox.ItemsSource = typeOptions;

            this.WidgetNameComboBox.ItemsSource = ChannelSession.Settings.OverlayWidgets.ToList();

            this.EntranceAnimationComboBox.ItemsSource = EnumHelper.GetEnumNames<OverlayItemEffectEntranceAnimationTypeEnum>();
            this.EntranceAnimationComboBox.SelectedIndex = 0;
            this.VisibleAnimationComboBox.ItemsSource = EnumHelper.GetEnumNames<OverlayItemEffectVisibleAnimationTypeEnum>();
            this.VisibleAnimationComboBox.SelectedIndex = 0;
            this.ExitAnimationComboBox.ItemsSource = EnumHelper.GetEnumNames<OverlayItemEffectExitAnimationTypeEnum>();
            this.ExitAnimationComboBox.SelectedIndex = 0;

            if (this.action != null)
            {
                if (!string.IsNullOrEmpty(this.action.OverlayName))
                {
                    this.OverlayNameComboBox.SelectedItem = this.action.OverlayName;
                }

                if (this.action.WidgetID != Guid.Empty)
                {
                    this.TypeComboBox.SelectedItem = ShowHideWidgetOption;
                    this.WidgetNameComboBox.SelectedItem = ChannelSession.Settings.OverlayWidgets.FirstOrDefault(w => w.Item.ID.Equals(this.action.WidgetID));
                    this.ShowHideWidgetCheckBox.IsChecked = this.action.ShowWidget;
                }
                else
                {
#pragma warning disable CS0612 // Type or member is obsolete
                    if (this.action.Item != null)
                    {
                        StoreCommandUpgrader.RestructureNewerOverlayActions(new List<ActionBase>() { this.action });
                    }
#pragma warning restore CS0612 // Type or member is obsolete

                    if (this.action.OverlayItem != null)
                    {
                        if (this.action.OverlayItem.Effects != null)
                        {
                            this.DurationTextBox.Text = this.action.OverlayItem.Effects.Duration.ToString();
                            this.EntranceAnimationComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.OverlayItem.Effects.EntranceAnimation);
                            this.VisibleAnimationComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.OverlayItem.Effects.VisibleAnimation);
                            this.ExitAnimationComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.OverlayItem.Effects.ExitAnimation);
                        }

                        if (this.action.OverlayItem.Position != null)
                        {
                            this.ItemPosition.SetPosition(this.action.OverlayItem.Position);
                        }

                        if (this.action.OverlayItem is OverlayImageItemModel)
                        {
                            this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayItemModelTypeEnum.Image);
                            this.ImageItem.SetItem(this.action.OverlayItem);
                        }
                        else if (this.action.OverlayItem is OverlayTextItemModel)
                        {
                            this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayItemModelTypeEnum.Text);
                            this.TextItem.SetItem(this.action.OverlayItem);
                        }
                        else if (this.action.OverlayItem is OverlayYouTubeItemModel)
                        {
                            this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayItemModelTypeEnum.YouTube);
                            this.YouTubeItem.SetItem(this.action.OverlayItem);
                        }
                        else if (this.action.OverlayItem is OverlayVideoItemModel)
                        {
                            this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayItemModelTypeEnum.Video);
                            this.VideoItem.SetItem(this.action.OverlayItem);
                        }
                        else if (this.action.OverlayItem is OverlayWebPageItemModel)
                        {
                            this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayItemModelTypeEnum.WebPage);
                            this.WebPageItem.SetItem(this.action.OverlayItem);
                        }
                        else if (this.action.OverlayItem is OverlayHTMLItemModel)
                        {
                            this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayItemModelTypeEnum.HTML);
                            this.HTMLItem.SetItem(this.action.OverlayItem);
                        }
                    }
                }
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            string type = (string)this.TypeComboBox.SelectedItem;
            if (!string.IsNullOrEmpty(type))
            {
                if (type.Equals(ShowHideWidgetOption))
                {
                    if (this.WidgetNameComboBox.SelectedIndex >= 0)
                    {
                        OverlayWidgetModel widget = (OverlayWidgetModel)this.WidgetNameComboBox.SelectedItem;
                        return new OverlayAction(widget.Item.ID, this.ShowHideWidgetCheckBox.IsChecked.GetValueOrDefault());
                    }
                }
                else
                {
                    OverlayItemModelTypeEnum overlayType = EnumHelper.GetEnumValueFromString<OverlayItemModelTypeEnum>(type);

                    if (this.OverlayNameComboBox.SelectedIndex < 0)
                    {
                        return null;
                    }
                    string overlayName = (string)this.OverlayNameComboBox.SelectedItem;

                    double duration;
                    if (double.TryParse(this.DurationTextBox.Text, out duration) && duration > 0 && this.EntranceAnimationComboBox.SelectedIndex >= 0 &&
                        this.VisibleAnimationComboBox.SelectedIndex >= 0 && this.ExitAnimationComboBox.SelectedIndex >= 0)
                    {
                        OverlayItemEffectEntranceAnimationTypeEnum entrance = EnumHelper.GetEnumValueFromString<OverlayItemEffectEntranceAnimationTypeEnum>((string)this.EntranceAnimationComboBox.SelectedItem);
                        OverlayItemEffectVisibleAnimationTypeEnum visible = EnumHelper.GetEnumValueFromString<OverlayItemEffectVisibleAnimationTypeEnum>((string)this.VisibleAnimationComboBox.SelectedItem);
                        OverlayItemEffectExitAnimationTypeEnum exit = EnumHelper.GetEnumValueFromString<OverlayItemEffectExitAnimationTypeEnum>((string)this.ExitAnimationComboBox.SelectedItem);

                        OverlayItemEffectsModel effects = new OverlayItemEffectsModel(entrance, visible, exit, duration);

                        OverlayItemPositionModel position = this.ItemPosition.GetPosition();

                        OverlayItemModelBase item = null;

                        if (overlayType == OverlayItemModelTypeEnum.Image)
                        {
                            item = this.ImageItem.GetItem();
                        }
                        else if (overlayType == OverlayItemModelTypeEnum.Text)
                        {
                            item = this.TextItem.GetItem();
                        }
                        else if (overlayType == OverlayItemModelTypeEnum.YouTube)
                        {
                            item = this.YouTubeItem.GetItem();
                        }
                        else if (overlayType == OverlayItemModelTypeEnum.Video)
                        {
                            item = this.VideoItem.GetItem();
                        }
                        else if (overlayType == OverlayItemModelTypeEnum.WebPage)
                        {
                            item = this.WebPageItem.GetItem();
                        }
                        else if (overlayType == OverlayItemModelTypeEnum.HTML)
                        {
                            item = this.HTMLItem.GetItem();
                        }

                        if (item != null)
                        {
                            item.Position = position;
                            item.Effects = effects;

                            return new OverlayAction(overlayName, item);
                        }
                    }
                }
            }
            return null;
        }

        private void OverlayTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.OverlayNameComboBox.Visibility = Visibility.Collapsed;
            this.ShowHideWidgetGrid.Visibility = Visibility.Collapsed;
            this.ImageItem.Visibility = Visibility.Collapsed;
            this.TextItem.Visibility = Visibility.Collapsed;
            this.YouTubeItem.Visibility = Visibility.Collapsed;
            this.VideoItem.Visibility = Visibility.Collapsed;
            this.WebPageItem.Visibility = Visibility.Collapsed;
            this.HTMLItem.Visibility = Visibility.Collapsed;
            this.AdditionalOptionsGrid.Visibility = Visibility.Collapsed;
            if (this.TypeComboBox.SelectedIndex >= 0)
            {
                string type = (string)this.TypeComboBox.SelectedItem;
                if (type.Equals(ShowHideWidgetOption))
                {
                    this.ShowHideWidgetGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    this.OverlayNameComboBox.Visibility = Visibility.Visible;
                    OverlayItemModelTypeEnum overlayType = EnumHelper.GetEnumValueFromString<OverlayItemModelTypeEnum>(type);
                    if (overlayType == OverlayItemModelTypeEnum.Image)
                    {
                        this.ImageItem.Visibility = Visibility.Visible;
                    }
                    else if (overlayType == OverlayItemModelTypeEnum.Text)
                    {
                        this.TextItem.Visibility = Visibility.Visible;
                    }
                    else if (overlayType == OverlayItemModelTypeEnum.YouTube)
                    {
                        this.YouTubeItem.Visibility = Visibility.Visible;
                    }
                    else if (overlayType == OverlayItemModelTypeEnum.Video)
                    {
                        this.VideoItem.Visibility = Visibility.Visible;
                    }
                    else if (overlayType == OverlayItemModelTypeEnum.WebPage)
                    {
                        this.WebPageItem.Visibility = Visibility.Visible;
                    }
                    else if (overlayType == OverlayItemModelTypeEnum.HTML)
                    {
                        this.HTMLItem.Visibility = Visibility.Visible;
                    }
                    this.AdditionalOptionsGrid.Visibility = Visibility.Visible;
                }
            }
        }
    }
}
