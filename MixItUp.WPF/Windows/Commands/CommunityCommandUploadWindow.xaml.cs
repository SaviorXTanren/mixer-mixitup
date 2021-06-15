using MixItUp.Base;
using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Store;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Windows.Commands
{
    /// <summary>
    /// Interaction logic for CommunityCommandUploadWindow.xaml
    /// </summary>
    public partial class CommunityCommandUploadWindow : LoadingWindowBase
    {
        private CommandModelBase command;

        private CommunityCommandUploadModel uploadCommand;

        public CommunityCommandUploadWindow(CommandModelBase command)
        {
            InitializeComponent();

            this.command = command;

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            await base.OnLoaded();

            try
            {
                this.uploadCommand = new CommunityCommandUploadModel()
                {
                    ID = this.command.ID,
                    Name = this.command.Name,
                };

                this.uploadCommand.SetCommands(new List<CommandModelBase>() { this.command });

                CommunityCommandDetailsModel existingCommand = await ChannelSession.Services.CommunityCommandsService.GetCommandDetails(command.ID);
                if (existingCommand != null)
                {
                    this.uploadCommand.ID = existingCommand.ID;
                    this.uploadCommand.Description = existingCommand.Description;
                }

                HashSet<CommunityCommandTagEnum> tags = new HashSet<CommunityCommandTagEnum>();
                foreach (ActionModelBase action in this.command.Actions)
                {
                    if (action is ConditionalActionModel)
                    {
                        foreach (ActionModelBase subAction in ((ConditionalActionModel)action).Actions)
                        {
                            tags.Add((CommunityCommandTagEnum)subAction.Type);
                        }
                    }
                    tags.Add((CommunityCommandTagEnum)action.Type);
                }

                this.uploadCommand.Tags.Clear();
                foreach (CommunityCommandTagEnum tag in tags.Take(5))
                {
                    this.uploadCommand.Tags.Add(tag);
                }

                this.NameTextBox.Text = this.uploadCommand.Name;
                this.DescriptionTextBox.Text = this.uploadCommand.Description;

                if (tags.Contains(CommunityCommandTagEnum.File) || tags.Contains(CommunityCommandTagEnum.Overlay) || tags.Contains(CommunityCommandTagEnum.Sound))
                {
                    this.ExternalAssetsDetectedTextBlock.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private void BrowseImageFilePathButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog(ChannelSession.Services.FileService.ImageFileFilter());
            if (!string.IsNullOrEmpty(filePath))
            {
                this.ImageFilePathTextBox.Text = filePath;
            }
        }

        private async void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            this.StartLoadingOperation();

            try
            {
                if (string.IsNullOrEmpty(this.NameTextBox.Text))
                {
                    await DialogHelper.ShowMessage(MixItUp.Base.Resources.CommunityCommandsUploadInvalidName);
                    return;
                }

                if (string.IsNullOrEmpty(this.DescriptionTextBox.Text))
                {
                    await DialogHelper.ShowMessage(MixItUp.Base.Resources.CommunityCommandsUploadInvalidDescription);
                    return;
                }

                if (!string.IsNullOrEmpty(this.ImageFilePathTextBox.Text))
                {
                    if (!ChannelSession.Services.FileService.FileExists(this.ImageFilePathTextBox.Text))
                    {
                        await DialogHelper.ShowMessage(MixItUp.Base.Resources.CommunityCommandsUploadInvalidImageFile);
                        return;
                    }
                    this.uploadCommand.ImageFileData = await ChannelSession.Services.FileService.ReadFileAsBytes(this.ImageFilePathTextBox.Text);
                }

                this.uploadCommand.Name = this.NameTextBox.Text;
                this.uploadCommand.Description = this.DescriptionTextBox.Text;

                if (!string.IsNullOrEmpty(this.ImageFilePathTextBox.Text) && ChannelSession.Services.FileService.FileExists(this.ImageFilePathTextBox.Text))
                {
                    this.uploadCommand.ImageFileData = await ChannelSession.Services.FileService.ReadFileAsBytes(this.ImageFilePathTextBox.Text);
                }

                await ChannelSession.Services.CommunityCommandsService.AddOrUpdateCommand(this.uploadCommand);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            this.EndLoadingOperation();

            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
