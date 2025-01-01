using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Store;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.CommunityCommands;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
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
        private CommunityCommandDetailsViewModel ccCommand;

        private CommunityCommandUploadModel uploadCommand;

        private bool commandContainsScript;
        private bool commandContainsMedia;
        private bool commandReferencesOtherCommand;

        public CommunityCommandUploadWindow(CommandModelBase command)
        {
            InitializeComponent();

            this.command = command;

            this.Initialize(this.StatusBar);
        }

        public CommunityCommandUploadWindow(CommunityCommandDetailsViewModel ccCommand)
        {
            InitializeComponent();

            this.ccCommand = ccCommand;

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            await base.OnLoaded();

            try
            {
                if (this.command != null)
                {
                    this.uploadCommand = new CommunityCommandUploadModel()
                    {
                        ID = this.command.ID,
                        Name = this.command.Name,
                    };

                    this.uploadCommand.SetCommands(new List<CommandModelBase>() { this.command });

                    CommunityCommandDetailsModel existingCommand = await ServiceManager.Get<MixItUpService>().GetCommandDetails(command.ID);
                    if (existingCommand != null)
                    {
                        this.uploadCommand.ID = existingCommand.ID;
                        this.uploadCommand.Description = existingCommand.Description;
                    }

                    this.uploadCommand.Tags.Clear();
                    this.SetActionTags(this.uploadCommand.Tags, this.command.Actions);

                    switch (this.command.Type)
                    {
                        case CommandTypeEnum.Chat: this.uploadCommand.Tags.Add(CommunityCommandTagEnum.ChatCommand); break;
                        case CommandTypeEnum.Event: this.uploadCommand.Tags.Add(CommunityCommandTagEnum.EventCommand); break;
                        case CommandTypeEnum.ActionGroup: this.uploadCommand.Tags.Add(CommunityCommandTagEnum.ActionGroupCommand); break;
                        case CommandTypeEnum.Timer: this.uploadCommand.Tags.Add(CommunityCommandTagEnum.TimerCommand); break;
                        case CommandTypeEnum.Game: this.uploadCommand.Tags.Add(CommunityCommandTagEnum.GameCommand); break;
                        case CommandTypeEnum.StreamlootsCard: this.uploadCommand.Tags.Add(CommunityCommandTagEnum.StreamlootsCardCommand); break;
                        case CommandTypeEnum.TwitchChannelPoints: this.uploadCommand.Tags.Add(CommunityCommandTagEnum.TwitchChannelPointsCommand); break;
                        case CommandTypeEnum.Webhook: this.uploadCommand.Tags.Add(CommunityCommandTagEnum.Webhook); break;
                        case CommandTypeEnum.TrovoSpell: this.uploadCommand.Tags.Add(CommunityCommandTagEnum.TrovoSpell); break;
                        case CommandTypeEnum.TwitchBits: this.uploadCommand.Tags.Add(CommunityCommandTagEnum.TwitchBits); break;
                        case CommandTypeEnum.CrowdControlEffect: this.uploadCommand.Tags.Add(CommunityCommandTagEnum.CrowdControlEffect); break;
                    }
                }
                else
                {
                    this.uploadCommand = new CommunityCommandUploadModel()
                    {
                        ID = this.ccCommand.ID,
                        Name = this.ccCommand.Name,
                        Description = this.ccCommand.Description,
                    };
                }

                this.NameTextBox.Text = this.uploadCommand.Name;
                this.DescriptionTextBox.Text = this.uploadCommand.Description;

                if (this.uploadCommand.Tags.Contains(CommunityCommandTagEnum.Script))
                {
                    this.commandContainsScript = true;
                }

                if (this.uploadCommand.Tags.Contains(CommunityCommandTagEnum.Sound) || this.uploadCommand.Tags.Contains(CommunityCommandTagEnum.Overlay) ||
                    this.uploadCommand.Tags.Contains(CommunityCommandTagEnum.File) || this.uploadCommand.Tags.Contains(CommunityCommandTagEnum.ExternalProgram))
                {
                    this.commandContainsMedia = true;
                }

                if (this.uploadCommand.Tags.Contains(CommunityCommandTagEnum.Command))
                {
                    this.commandReferencesOtherCommand = true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private void SetActionTags(HashSet<CommunityCommandTagEnum> tags, IEnumerable<ActionModelBase> actions)
        {
            foreach (ActionModelBase action in actions)
            {
                if (action is GroupActionModel)
                {
                    this.SetActionTags(tags, ((GroupActionModel)action).Actions);
                }
                this.uploadCommand.Tags.Add((CommunityCommandTagEnum)action.Type);
            }
        }

        private void BrowseImageFilePathButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = ServiceManager.Get<IFileService>().ShowOpenFileDialog(ServiceManager.Get<IFileService>().ImageFileFilter());
            if (!string.IsNullOrEmpty(filePath))
            {
                this.ImageFilePathTextBox.Text = filePath;
            }
        }

        private void BrowseScreenshotFilePathButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = ServiceManager.Get<IFileService>().ShowOpenFileDialog(ServiceManager.Get<IFileService>().ImageFileFilter());
            if (!string.IsNullOrEmpty(filePath))
            {
                this.ScreenshotFilePathTextBox.Text = filePath;
            }
        }

        private async void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                try
                {
                    if (this.commandContainsScript)
                    {
                        await DialogHelper.ShowMessage(MixItUp.Base.Resources.CommunityCommandsScriptActionsNotSupported);
                        return;
                    }

                    if (this.commandContainsMedia)
                    {
                        if (!await DialogHelper.ShowConfirmation(MixItUp.Base.Resources.CommunityCommandsExternalAssetActionsDetected))
                        {
                            return;
                        }
                    }

                    if (this.commandReferencesOtherCommand)
                    {
                        if (!await DialogHelper.ShowConfirmation(MixItUp.Base.Resources.CommunityCommandsOtherCommandReferencedDetected))
                        {
                            return;
                        }
                    }

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
                        if (!ServiceManager.Get<IFileService>().FileExists(this.ImageFilePathTextBox.Text))
                        {
                            await DialogHelper.ShowMessage(MixItUp.Base.Resources.CommunityCommandsUploadInvalidImageFile);
                            return;
                        }
                    }

                    if (!string.IsNullOrEmpty(this.ScreenshotFilePathTextBox.Text))
                    {
                        if (!ServiceManager.Get<IFileService>().FileExists(this.ScreenshotFilePathTextBox.Text))
                        {
                            await DialogHelper.ShowMessage(MixItUp.Base.Resources.CommunityCommandsUploadInvalidImageFile);
                            return;
                        }
                    }

                    this.uploadCommand.Name = this.NameTextBox.Text;
                    this.uploadCommand.Description = this.DescriptionTextBox.Text;

                    if (!await DialogHelper.ShowConfirmation(MixItUp.Base.Resources.CommunityCommandsUploadAgreement))
                    {
                        this.Close();
                        return;
                    }

                    if (!string.IsNullOrEmpty(this.ImageFilePathTextBox.Text) && ServiceManager.Get<IFileService>().FileExists(this.ImageFilePathTextBox.Text))
                    {
                        string imageFilePath = this.ImageFilePathTextBox.Text;
                        this.uploadCommand.ImageFileData = await Task.Run(() =>
                        {
                            try
                            {
                                using (var image = Image.Load(imageFilePath))
                                {
                                    image.Mutate(i => i.Resize(100, 100));
                                    using (MemoryStream memoryStream = new MemoryStream())
                                    {
                                        image.SaveAsPng(memoryStream);
                                        return memoryStream.ToArray();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Log(ex);
                            }
                            return null;
                        });
                    }

                    if (!string.IsNullOrEmpty(this.ScreenshotFilePathTextBox.Text) && ServiceManager.Get<IFileService>().FileExists(this.ScreenshotFilePathTextBox.Text))
                    {
                        string screenshotFilePath = this.ScreenshotFilePathTextBox.Text;
                        this.uploadCommand.ScreenshotFileData = await Task.Run(() =>
                        {
                            try
                            {
                                using (var image = Image.Load(screenshotFilePath))
                                {
                                    if (image.Width > 1280)
                                    {
                                        image.Mutate(i => i.Resize(1280, 0));
                                    }
                                    if (image.Height > 720)
                                    {
                                        image.Mutate(i => i.Resize(0, 720));
                                    }

                                    using (MemoryStream memoryStream = new MemoryStream())
                                    {
                                        image.SaveAsPng(memoryStream);
                                        return memoryStream.ToArray();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Log(ex);
                            }
                            return null;
                        });
                    }

                    await ServiceManager.Get<MixItUpService>().AddOrUpdateCommand(this.uploadCommand);

                    this.Close();
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);

                    await DialogHelper.ShowMessage(MixItUp.Base.Resources.CommunityCommandsFailedToUploadCommand + ex.Message);
                }
            });
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
