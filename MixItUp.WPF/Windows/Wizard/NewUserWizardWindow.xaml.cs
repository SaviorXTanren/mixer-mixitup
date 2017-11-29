using Mixer.Base.Model.OAuth;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.ScorpBot;
using MixItUp.Base.ViewModel.User;
using MixItUp.Desktop.Database;
using MixItUp.WPF.Util;
using System;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace MixItUp.WPF.Windows.Wizard
{
    /// <summary>
    /// Interaction logic for NewUserWizardWindow.xaml
    /// </summary>
    public partial class NewUserWizardWindow : LoadingWindowBase
    {
        private string directoryPath;

        private ScorpBotData scorpBotData;

        public NewUserWizardWindow()
        {
            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override Task OnLoaded()
        {
            this.directoryPath = AppDomain.CurrentDomain.BaseDirectory;
            this.XSplitExtensionPathTextBox.Text = Path.Combine(this.directoryPath, "XSplit\\Mix It Up.html");

            return base.OnLoaded();
        }

        protected override void OnClosed(EventArgs e)
        {
            MainWindow window = new MainWindow();
            window.Show();
            this.Close();

            base.OnClosed(e);
        }

        private void BackButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.ImportSettingsPageGrid.Visibility == System.Windows.Visibility.Visible)
            {
                this.BackButton.IsEnabled = false;
                this.ImportSettingsPageGrid.Visibility = System.Windows.Visibility.Collapsed;
                this.IntroPageGrid.Visibility = System.Windows.Visibility.Visible;
            }
            else if (this.ExternalServicesPageGrid.Visibility == System.Windows.Visibility.Visible)
            {
                this.ExternalServicesPageGrid.Visibility = System.Windows.Visibility.Collapsed;
                this.ImportSettingsPageGrid.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private async void NextButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                if (this.IntroPageGrid.Visibility == System.Windows.Visibility.Visible)
                {
                    this.BackButton.IsEnabled = true;
                    this.IntroPageGrid.Visibility = System.Windows.Visibility.Collapsed;
                    this.ImportSettingsPageGrid.Visibility = System.Windows.Visibility.Visible;
                }
                else if (this.ImportSettingsPageGrid.Visibility == System.Windows.Visibility.Visible)
                {
                    if (!string.IsNullOrEmpty(this.ScorpBotDirectoryTextBox.Text))
                    {
                        this.scorpBotData = await this.GatherScorpBotData(this.ScorpBotDirectoryTextBox.Text);
                        if (this.scorpBotData == null)
                        {
                            await MessageBoxHelper.ShowMessageDialog("Failed to import ScorpBot settings, please ensure that you have selected the correct directory. If this continues to fail, please contact Mix it Up support for assitance.");
                            return;
                        }
                    }

                    this.ImportSettingsPageGrid.Visibility = System.Windows.Visibility.Collapsed;
                    this.ExternalServicesPageGrid.Visibility = System.Windows.Visibility.Visible;
                }
                else if (this.ExternalServicesPageGrid.Visibility == System.Windows.Visibility.Visible)
                {
                    this.FinalizeNewUser();
                    this.Close();
                }
            });
        }

        private void ScorpBotDirectoryBrowseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string folderPath = ChannelSession.Services.FileService.ShowOpenFolderDialog();
            if (!string.IsNullOrEmpty(folderPath))
            {
                this.ScorpBotDirectoryTextBox.Text = folderPath;
            }
        }

        private async Task<ScorpBotData> GatherScorpBotData(string folderPath)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    ScorpBotData scorpBotData = new ScorpBotData();

                    string dataPath = Path.Combine(folderPath, "Data\\Database");
                    if (Directory.Exists(dataPath))
                    {
                        SQLiteDatabaseWrapper databaseWrapper = new SQLiteDatabaseWrapper(dataPath);

                        databaseWrapper.DatabaseFilePath = Path.Combine(dataPath, "CommandsDB.sqlite");
                        await databaseWrapper.ReadRows("SELECT * FROM RegCommand",
                        (reader) =>
                        {
                            if (ScorpBotCommand.IsACommand(reader))
                            {
                                scorpBotData.Commands.Add(new ScorpBotCommand(reader));
                            }
                        });

                        databaseWrapper.DatabaseFilePath = Path.Combine(dataPath, "FilteredWordsDB.sqlite");
                        await databaseWrapper.ReadRows("SELECT * FROM Word",
                        (reader) =>
                        {
                            scorpBotData.BannedWords.Add((string)reader["word"]);
                        });

                        databaseWrapper.DatabaseFilePath = Path.Combine(dataPath, "QuotesDB.sqlite");
                        await databaseWrapper.ReadRows("SELECT * FROM Quotes",
                        (reader) =>
                        {
                            scorpBotData.Quotes.Add((string)reader["quote_text"]);
                        });

                        databaseWrapper.DatabaseFilePath = Path.Combine(dataPath, "RankDB.sqlite");
                        await databaseWrapper.ReadRows("SELECT * FROM Rank",
                        (reader) =>
                        {
                            scorpBotData.Ranks.Add(new ScorpBotRank(reader));
                        });

                        databaseWrapper.DatabaseFilePath = Path.Combine(dataPath, "Viewers3DB.sqlite");
                        await databaseWrapper.ReadRows("SELECT * FROM Viewer",
                        (reader) =>
                        {
                            scorpBotData.Viewers.Add(new ScorpBotViewer(reader));
                        });
                    }

                    return scorpBotData;
                }
                catch (Exception ex) { Logger.Log(ex); }
                return null;
            });
        }

        private async void BotLogInButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            bool result = await this.RunAsyncOperation(async () =>
            {
                return await ChannelSession.ConnectBot((OAuthShortCodeModel shortCode) =>
                {
                    this.BotLoginShortCodeTextBox.Text = shortCode.code;

                    Process.Start("https://mixer.com/oauth/shortcode?approval_prompt=force&code=" + shortCode.code);
                });
            });

            if (result)
            {
                this.BotLoggedInNameTextBlock.Text = ChannelSession.BotUser.username;
                if (!string.IsNullOrEmpty(ChannelSession.BotUser.avatarUrl))
                {
                    this.BotProfileAvatar.SetImageUrl(ChannelSession.BotUser.avatarUrl);
                }

                this.BotLogInGrid.Visibility = System.Windows.Visibility.Hidden;
                this.BotLoggedInGrid.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void InstallOBSStudioPlugin_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string pluginPath = Path.Combine(this.directoryPath, "OBS\\obs-websocket-4.2.0-Windows-Installer.exe");
            if (File.Exists(pluginPath))
            {
                Process.Start(pluginPath);
            }
        }

        private async void ConnectToOBSStudioButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ChannelSession.Settings.OBSStudioServerIP = ChannelSession.DefaultOBSStudioConnection;
            bool result = await this.RunAsyncOperation(async () =>
            {
                return await ChannelSession.Services.InitializeOBSWebsocket();
            });

            if (result)
            {
                this.OBSStudioConnectedSuccessfulTextBlock.Visibility = System.Windows.Visibility.Visible;
                this.ConnectToOBSStudioButton.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                await MessageBoxHelper.ShowMessageDialog("Could not connect to OBS Studio. Please make sure OBS Studio is running, the obs-websocket plugin is installed, and the connection and password are set to their default settings. If you wish to connect with a specific port and password, you'll need to do this out of the wizard.");
            }
        }

        private async void ConnectToXSplitButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            bool result = await this.RunAsyncOperation(async () =>
            {
                return await ChannelSession.Services.InitializeXSplitServer();
            });

            if (result)
            {
                ChannelSession.Settings.EnableXSplitConnection = true;
                this.XSplitConnectedSuccessfulTextBlock.Visibility = System.Windows.Visibility.Visible;
                this.ConnectToXSplitButton.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                await this.RunAsyncOperation(async () =>
                {
                    await ChannelSession.Services.DisconnectXSplitServer();
                });
                await MessageBoxHelper.ShowMessageDialog("Could not connect to XSplit. Please make sure XSplit is running, the Mix It Up plugin is installed, and is running");
            }
        }

        private void FinalizeNewUser()
        {
            if (this.scorpBotData != null)
            {
                foreach (ScorpBotViewer viewer in this.scorpBotData.Viewers)
                {
                    ChannelSession.Settings.UserData[viewer.ID] = new UserViewModel(viewer);
                }

                foreach (ScorpBotCommand command in this.scorpBotData.Commands)
                {
                    ChannelSession.Settings.ChatCommands.Add(new ChatCommand(command));
                }

                foreach (string quote in this.scorpBotData.Quotes)
                {
                    ChannelSession.Settings.Quotes.Add(quote);
                }

                if (ChannelSession.Settings.Quotes.Count > 0)
                {
                    ChannelSession.Settings.QuotesEnabled = true;
                }

                foreach (string bannedWord in this.scorpBotData.BannedWords)
                {
                    ChannelSession.Settings.BannedWords.Add(bannedWord);
                }
            }
        }
    }
}
