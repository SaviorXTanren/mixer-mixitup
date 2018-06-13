using Mixer.Base.Clients;
using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Store;
using MixItUp.WPF.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.WPF.Windows.Store
{
    /// <summary>
    /// Interaction logic for UploadStoreListingWindow.xaml
    /// </summary>
    public partial class UploadStoreListingWindow : LoadingWindowBase
    {
        private static readonly string IncludeAssetsTooltip =
            "This option will upload the additional files used in" + Environment.NewLine +
            "the actions for this command (Images, Sounds, etc)." + Environment.NewLine +
            "Some files are not support by this (Videos, HTML, etc).";

        private CommandBase command;

        public UploadStoreListingWindow(CommandBase command)
        {
            this.command = command;

            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override Task OnLoaded()
        {
            List<string> tags = new List<string>(StoreListingModel.ValidTags);
            tags.Insert(0, string.Empty);
            this.Tag1ComboBox.ItemsSource = this.Tag2ComboBox.ItemsSource = this.Tag3ComboBox.ItemsSource = this.Tag4ComboBox.ItemsSource =
                this.Tag5ComboBox.ItemsSource = this.Tag6ComboBox.ItemsSource = tags;
            this.Tag1ComboBox.SelectedIndex = this.Tag2ComboBox.SelectedIndex = this.Tag3ComboBox.SelectedIndex = this.Tag4ComboBox.SelectedIndex =
                this.Tag5ComboBox.SelectedIndex = this.Tag6ComboBox.SelectedIndex = 0;

            this.IncludeAssetsTextBlock.ToolTip = this.IncludeAssetsToggleButton.ToolTip = IncludeAssetsTooltip;

            this.NameTextBox.Text = this.command.Name;

            Dictionary<ActionTypeEnum, int> actionsInCommand = new Dictionary<ActionTypeEnum, int>();
            foreach (ActionBase action in this.command.Actions)
            {
                if (!actionsInCommand.ContainsKey(action.Type))
                {
                    actionsInCommand[action.Type] = 0;
                }
                actionsInCommand[action.Type]++;
            }

            IEnumerable<ActionTypeEnum> actionTypesInCommand = actionsInCommand.OrderBy(a => Guid.NewGuid()).OrderByDescending(a => a.Value).Select(a => a.Key);
            for (int i = 0; i < 3 && i < actionTypesInCommand.Count(); i++)
            {
                string tag = EnumHelper.GetEnumName(actionTypesInCommand.ElementAt(i));
                switch (i)
                {
                    case 0: this.Tag1ComboBox.SelectedItem = tag; break;
                    case 1: this.Tag2ComboBox.SelectedItem = tag; break;
                    case 2: this.Tag3ComboBox.SelectedItem = tag; break;
                }
            }

            if (this.command is EventCommand)
            {
                EventCommand eCommand = (EventCommand)this.command;
                switch (eCommand.EventType)
                {
                    case ConstellationEventTypeEnum.channel__id__followed: this.Tag4ComboBox.SelectedItem = StoreListingModel.FollowTag; break;
                    case ConstellationEventTypeEnum.channel__id__hosted: this.Tag4ComboBox.SelectedItem = StoreListingModel.HostTag; break;
                    case ConstellationEventTypeEnum.channel__id__subscribed: this.Tag4ComboBox.SelectedItem = StoreListingModel.SubscribeTag; break;
                    case ConstellationEventTypeEnum.channel__id__resubscribed: this.Tag4ComboBox.SelectedItem = StoreListingModel.ResubscribeTag; break;
                }

                switch (eCommand.OtherEventType)
                {
                    case OtherEventTypeEnum.StreamlabsDonation:
                    case OtherEventTypeEnum.GawkBoxDonation:
                    case OtherEventTypeEnum.TiltifyDonation:
                        this.Tag4ComboBox.SelectedItem = StoreListingModel.DonationTag;
                        break;
                }
            }

            return base.OnLoaded();
        }

        private void DisplayImageBrowseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog(ChannelSession.Services.FileService.ImageFileFilter());
            if (!string.IsNullOrEmpty(filePath))
            {
                this.DisplayImagePathTextBox.Text = filePath;
            }
        }

        private async void UploadButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.NameTextBox.Text))
            {
                await MessageBoxHelper.ShowMessageDialog("A name must be specified");
                return;
            }

            if (string.IsNullOrEmpty(this.DescriptionTextBox.Text))
            {
                await MessageBoxHelper.ShowMessageDialog("A description must be specified");
                return;
            }

            if (!string.IsNullOrEmpty(this.DisplayImagePathTextBox.Text) && File.Exists(this.DisplayImagePathTextBox.Text))
            {
                await MessageBoxHelper.ShowMessageDialog("The specified display image does not exist");
                return;
            }

            List<string> tags = new List<string>();
            if (this.Tag1ComboBox.SelectedIndex >= 0) { tags.Add(this.Tag1ComboBox.Text); }
            if (this.Tag2ComboBox.SelectedIndex >= 0) { tags.Add(this.Tag2ComboBox.Text); }
            if (this.Tag3ComboBox.SelectedIndex >= 0) { tags.Add(this.Tag3ComboBox.Text); }
            if (this.Tag4ComboBox.SelectedIndex >= 0) { tags.Add(this.Tag4ComboBox.Text); }
            if (this.Tag5ComboBox.SelectedIndex >= 0) { tags.Add(this.Tag5ComboBox.Text); }
            if (this.Tag6ComboBox.SelectedIndex >= 0) { tags.Add(this.Tag6ComboBox.Text); }

            if (tags.Count == 0)
            {
                await MessageBoxHelper.ShowMessageDialog("At least 1 tag must be selected");
                return;
            }

            byte[] displayImageData = null;
            if (!string.IsNullOrEmpty(this.DisplayImagePathTextBox.Text))
            {
                displayImageData = await ChannelSession.Services.FileService.ReadFileAsBytes(this.DisplayImagePathTextBox.Text);
            }

            byte[] assetData = null;
            if (this.IncludeAssetsToggleButton.IsChecked.GetValueOrDefault())
            {
                List<string> assetFiles = new List<string>();
                foreach (ActionBase action in this.command.Actions)
                {
                    if (action.Type == ActionTypeEnum.Overlay)
                    {
                        OverlayAction oAction = (OverlayAction)action;
                        if (oAction.Effect is OverlayImageEffect)
                        {
                            OverlayImageEffect iEffect = (OverlayImageEffect)oAction.Effect;
                            assetFiles.Add(iEffect.FilePath);
                        }
                    }
                    else if (action.Type == ActionTypeEnum.Sound)
                    {
                        SoundAction sAction = (SoundAction)action;
                        assetFiles.Add(sAction.FilePath);
                    }
                }

                if (assetFiles.Count > 0)
                {
                    string zipFilePath = Path.Combine(ChannelSession.Services.FileService.GetTempFolder(), this.command.ID.ToString() + ".zip");
                    await ChannelSession.Services.FileService.ZipFiles(zipFilePath, assetFiles);
                    assetData = await ChannelSession.Services.FileService.ReadFileAsBytes(zipFilePath);
                }
            }

            await ChannelSession.Services.MixItUpService.AddStoreListing(new StoreDetailListingModel(this.command, this.NameTextBox.Text, this.DescriptionTextBox.Text, tags,
                displayImageData, assetData));

            this.Close();
        }
    }
}
