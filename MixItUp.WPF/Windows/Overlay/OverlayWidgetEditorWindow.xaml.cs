using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Model.Overlay;
using MixItUp.WPF.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Windows.Overlay
{
    public enum OverlayWidgetTypeEnum
    {
        Text,
        Image,
        Video,
        YouTube,
        HTML,
        [Name("Web Page")]
        WebPage,
        [Name("Goal/Progress Bar")]
        ProgressBar,
        [Name("Event List")]
        EventList,
    }

    /// <summary>
    /// Interaction logic for OverlayWidgetEditorWindow.xaml
    /// </summary>
    public partial class OverlayWidgetEditorWindow : LoadingWindowBase
    {
        public OverlayWidget Widget { get; private set; }

        public OverlayWidgetEditorWindow()
        {
            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        public OverlayWidgetEditorWindow(OverlayWidget widget)
            : this()
        {
            this.Widget = widget;
        }

        protected override async Task OnLoaded()
        {
            this.TypeComboBox.ItemsSource = EnumHelper.GetEnumNames<OverlayWidgetTypeEnum>();

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

            if (this.Widget != null)
            {
                this.NameTextBox.Text = this.Widget.Name;
                this.OverlayNameComboBox.SelectedItem = this.Widget.OverlayName;

                this.ItemPosition.SetPosition(this.Widget.Position);

                if (this.Widget.Item is OverlayImageItem)
                {
                    this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayWidgetTypeEnum.Image);
                    this.ImageItem.SetItem(this.Widget.Item);
                }
                else if (this.Widget.Item is OverlayTextItem)
                {
                    this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayWidgetTypeEnum.Text);
                    this.TextItem.SetItem(this.Widget.Item);
                }
                else if (this.Widget.Item is OverlayYouTubeItem)
                {
                    this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayWidgetTypeEnum.YouTube);
                    this.YouTubeItem.SetItem(this.Widget.Item);
                }
                else if (this.Widget.Item is OverlayVideoItem)
                {
                    this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayWidgetTypeEnum.Video);
                    this.VideoItem.SetItem(this.Widget.Item);
                }
                else if (this.Widget.Item is OverlayWebPageItem)
                {
                    this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayWidgetTypeEnum.WebPage);
                    this.WebPageItem.SetItem(this.Widget.Item);
                }
                else if (this.Widget.Item is OverlayHTMLItem)
                {
                    this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayWidgetTypeEnum.HTML);
                    this.HTMLItem.SetItem(this.Widget.Item);
                }
                else if (this.Widget.Item is OverlayProgressBarItem)
                {
                    this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayWidgetTypeEnum.ProgressBar);
                    this.ProgressBarItem.SetItem(this.Widget.Item);
                }
                else if (this.Widget.Item is OverlayEventList)
                {
                    this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(OverlayWidgetTypeEnum.ProgressBar);
                    this.ProgressBarItem.SetItem(this.Widget.Item);
                }
            }

            await base.OnLoaded();
        }

        private void TypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            this.ImageItem.Visibility = Visibility.Collapsed;
            this.TextItem.Visibility = Visibility.Collapsed;
            this.YouTubeItem.Visibility = Visibility.Collapsed;
            this.VideoItem.Visibility = Visibility.Collapsed;
            this.WebPageItem.Visibility = Visibility.Collapsed;
            this.HTMLItem.Visibility = Visibility.Collapsed;
            this.ProgressBarItem.Visibility = Visibility.Collapsed;
            this.EventListItem.Visibility = Visibility.Collapsed;
            if (this.TypeComboBox.SelectedIndex >= 0)
            {
                OverlayWidgetTypeEnum overlayType = EnumHelper.GetEnumValueFromString<OverlayWidgetTypeEnum>((string)this.TypeComboBox.SelectedItem);
                if (overlayType == OverlayWidgetTypeEnum.Image)
                {
                    this.ImageItem.Visibility = Visibility.Visible;
                }
                else if (overlayType == OverlayWidgetTypeEnum.Text)
                {
                    this.TextItem.Visibility = Visibility.Visible;
                }
                else if (overlayType == OverlayWidgetTypeEnum.YouTube)
                {
                    this.YouTubeItem.Visibility = Visibility.Visible;
                }
                else if (overlayType == OverlayWidgetTypeEnum.Video)
                {
                    this.VideoItem.Visibility = Visibility.Visible;
                }
                else if (overlayType == OverlayWidgetTypeEnum.WebPage)
                {
                    this.WebPageItem.Visibility = Visibility.Visible;
                }
                else if (overlayType == OverlayWidgetTypeEnum.HTML)
                {
                    this.HTMLItem.Visibility = Visibility.Visible;
                }
                else if (overlayType == OverlayWidgetTypeEnum.ProgressBar)
                {
                    this.ProgressBarItem.Visibility = Visibility.Visible;
                }
                else if (overlayType == OverlayWidgetTypeEnum.EventList)
                {
                    this.EventListItem.Visibility = Visibility.Visible;
                }
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                if (string.IsNullOrEmpty(this.NameTextBox.Text))
                {
                    await MessageBoxHelper.ShowMessageDialog("A name must be specified");
                    return;
                }

                if (this.TypeComboBox.SelectedIndex < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("A widget type must be selected");
                    return;
                }

                if (this.OverlayNameComboBox.SelectedIndex < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("An overlay to use must be selected");
                    return;
                }
                string overlayName = (string)this.OverlayNameComboBox.SelectedItem;

                OverlayItemPosition position = this.ItemPosition.GetPosition();

                OverlayItemBase item = null;
                OverlayWidgetTypeEnum overlayType = EnumHelper.GetEnumValueFromString<OverlayWidgetTypeEnum>((string)this.TypeComboBox.SelectedItem);
                if (overlayType == OverlayWidgetTypeEnum.Image)
                {
                    item = this.ImageItem.GetItem();
                }
                else if (overlayType == OverlayWidgetTypeEnum.Text)
                {
                    item = this.TextItem.GetItem();
                }
                else if (overlayType == OverlayWidgetTypeEnum.YouTube)
                {
                    item = this.YouTubeItem.GetItem();
                }
                else if (overlayType == OverlayWidgetTypeEnum.Video)
                {
                    item = this.VideoItem.GetItem();
                }
                else if (overlayType == OverlayWidgetTypeEnum.WebPage)
                {
                    item = this.WebPageItem.GetItem();
                }
                else if (overlayType == OverlayWidgetTypeEnum.HTML)
                {
                    item = this.HTMLItem.GetItem();
                }
                else if (overlayType == OverlayWidgetTypeEnum.ProgressBar)
                {
                    item = this.ProgressBarItem.GetItem();
                }
                else if (overlayType == OverlayWidgetTypeEnum.EventList)
                {
                    item = this.EventListItem.GetItem();
                }

                if (item == null)
                {
                    await MessageBoxHelper.ShowMessageDialog("There are missing Widget details");
                    return;
                }

                if (this.Widget == null)
                {
                    this.Widget = new OverlayWidget(this.NameTextBox.Text, overlayName, item, position);
                    ChannelSession.Settings.OverlayWidgets.Add(this.Widget);
                }
                else
                {
                    item.ID = this.Widget.Item.ID;

                    this.Widget.Name = this.NameTextBox.Text;
                    this.Widget.OverlayName = overlayName;
                    this.Widget.Item = item;
                    this.Widget.Position = position;
                }

                this.Close();
            });
        }
    }
}
