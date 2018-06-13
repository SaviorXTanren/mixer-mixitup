using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Store;
using MixItUp.WPF.Util;
using System;
using System.Collections.Generic;
using System.IO;
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
        }
    }
}
