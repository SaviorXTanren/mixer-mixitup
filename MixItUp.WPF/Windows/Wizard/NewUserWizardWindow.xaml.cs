using Mixer.Base.Interactive;
using Mixer.Base.Model.MixPlay;
using Mixer.Base.Model.User;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Import;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using MixItUp.Desktop.Database;
using MixItUp.WPF.Util;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MixItUp.WPF.Windows.Wizard
{
    public class SoundwaveProfileItem
    {
        public string Name { get; set; }
        public bool AddProfile { get; set; }
        public bool CanBeImported { get; set; }
    }

    /// <summary>
    /// Interaction logic for NewUserWizardWindow.xaml
    /// </summary>
    public partial class NewUserWizardWindow : LoadingWindowBase
    {
        private const string SoundwaveInteractiveGameName = "Soundwave Interactive Soundboard";
        private const string SoundwaveInteractiveCooldownGroupName = "Soundwave Buttons";

        private static readonly string SoundwaveInteractiveAppDataSettingsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Soundwave Interactive");
        private static readonly string FirebotAppDataSettingsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Firebot\\firebot-data\\user-settings");

        private string directoryPath;

        private IEnumerable<MixPlayGameListingModel> interactiveGames;

        private ScorpBotData scorpBotData;

        private StreamlabsChatBotData streamlabsChatBotData;

        private SoundwaveSettings soundwaveData;
        private List<SoundwaveProfileItem> soundwaveProfiles;

        public NewUserWizardWindow()
        {
            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            this.directoryPath = AppDomain.CurrentDomain.BaseDirectory;
            this.XSplitExtensionPathTextBox.Text = Path.Combine(this.directoryPath, "XSplit\\Mix It Up.html");
            this.IntroPageGrid.Visibility = System.Windows.Visibility.Visible;

            this.interactiveGames = await ChannelSession.MixerStreamerConnection.GetOwnedMixPlayGames(ChannelSession.MixerChannel);

            await this.GatherSoundwaveSettings();
            if (this.soundwaveData != null)
            {
                this.soundwaveProfiles = this.soundwaveData.Profiles.Keys.Select(p => new SoundwaveProfileItem() { Name = p, AddProfile = false, CanBeImported = !this.interactiveGames.Any(g => g.name.Equals(p)) }).ToList();
                this.SoundwaveInteractiveProfilesDataGrid.ItemsSource = this.soundwaveProfiles;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            ChannelSession.Settings.ReRunWizard = false;

            ShowMainWindow(new MainWindow());

            base.OnClosing(e);
        }

        private void BackButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.BotLoginPageGrid.Visibility == System.Windows.Visibility.Visible)
            {
                this.BotLoginPageGrid.Visibility = System.Windows.Visibility.Collapsed;
                this.IntroPageGrid.Visibility = System.Windows.Visibility.Visible;
            }
            else if (this.ExternalServicesPageGrid.Visibility == System.Windows.Visibility.Visible)
            {
                this.ExternalServicesPageGrid.Visibility = System.Windows.Visibility.Collapsed;
                this.BotLoginPageGrid.Visibility = System.Windows.Visibility.Visible;
            }
            else if (this.ImportScorpBotSettingsPageGrid.Visibility == System.Windows.Visibility.Visible)
            {
                this.ImportScorpBotSettingsPageGrid.Visibility = System.Windows.Visibility.Collapsed;
                this.ExternalServicesPageGrid.Visibility = System.Windows.Visibility.Visible;
            }
            else if (this.ImportStreamlabsChatBotSettingsPageGrid.Visibility == System.Windows.Visibility.Visible)
            {
                this.ImportStreamlabsChatBotSettingsPageGrid.Visibility = System.Windows.Visibility.Collapsed;
                this.ImportScorpBotSettingsPageGrid.Visibility = System.Windows.Visibility.Visible;
            }
            else if (this.ImportSoundwaveInteractiveSettingsGrid.Visibility == System.Windows.Visibility.Visible)
            {
                this.ImportSoundwaveInteractiveSettingsGrid.Visibility = System.Windows.Visibility.Collapsed;
                this.ImportStreamlabsChatBotSettingsPageGrid.Visibility = System.Windows.Visibility.Visible;
            }
            else if (this.SetupCompletePageGrid.Visibility == System.Windows.Visibility.Visible)
            {
                this.SetupCompletePageGrid.Visibility = System.Windows.Visibility.Collapsed;
                if (this.soundwaveData != null)
                {
                    this.ImportSoundwaveInteractiveSettingsGrid.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    this.ImportScorpBotSettingsPageGrid.Visibility = System.Windows.Visibility.Visible;
                }
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
                    this.BotLoginPageGrid.Visibility = System.Windows.Visibility.Visible;
                }
                else if (this.BotLoginPageGrid.Visibility == System.Windows.Visibility.Visible)
                {
                    this.BotLoginPageGrid.Visibility = System.Windows.Visibility.Collapsed;
                    this.ExternalServicesPageGrid.Visibility = System.Windows.Visibility.Visible;
                }
                else if (this.ExternalServicesPageGrid.Visibility == System.Windows.Visibility.Visible)
                {
                    this.ExternalServicesPageGrid.Visibility = System.Windows.Visibility.Collapsed;
                    this.ImportScorpBotSettingsPageGrid.Visibility = System.Windows.Visibility.Visible;
                }
                else if (this.ImportScorpBotSettingsPageGrid.Visibility == System.Windows.Visibility.Visible)
                {
                    this.StatusMessageTextBlock.Text = "Gathering ScorpBot Data...";
                    if (!string.IsNullOrEmpty(this.ScorpBotDirectoryTextBox.Text))
                    {
                        this.scorpBotData = await this.GatherScorpBotData(this.ScorpBotDirectoryTextBox.Text);
                        if (this.scorpBotData == null)
                        {
                            await MessageBoxHelper.ShowMessageDialog("Failed to import ScorpBot data, please ensure that you have selected the correct directory. If this continues to fail, please contact Mix it Up support for assitance.");
                            return;
                        }
                    }

                    this.ImportScorpBotSettingsPageGrid.Visibility = System.Windows.Visibility.Collapsed;
                    this.ImportStreamlabsChatBotSettingsPageGrid.Visibility = System.Windows.Visibility.Visible;
                }
                else if (this.ImportStreamlabsChatBotSettingsPageGrid.Visibility == System.Windows.Visibility.Visible)
                {
                    this.StatusMessageTextBlock.Text = "Gathering Streamlabs Chat Bot Data...";
                    if (!string.IsNullOrEmpty(this.StreamlabsChatBotDataFilePathTextBox.Text))
                    {
                        this.streamlabsChatBotData = await this.GatherStreamlabsChatBotSettings(this.StreamlabsChatBotDataFilePathTextBox.Text);
                        if (this.streamlabsChatBotData == null)
                        {
                            await MessageBoxHelper.ShowMessageDialog("Failed to import Streamlabs Chat Bot data, please ensure that you have selected the correct data file & have Microsoft Excel installed. If this continues to fail, please contact Mix it Up support for assitance.");
                            return;
                        }
                    }

                    this.ImportStreamlabsChatBotSettingsPageGrid.Visibility = System.Windows.Visibility.Collapsed;
                    if (this.soundwaveData != null)
                    {
                        this.ImportSoundwaveInteractiveSettingsGrid.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        this.SetupCompletePageGrid.Visibility = System.Windows.Visibility.Visible;
                    }
                }
                else if (this.ImportSoundwaveInteractiveSettingsGrid.Visibility == System.Windows.Visibility.Visible)
                {
                    this.ImportSoundwaveInteractiveSettingsGrid.Visibility = System.Windows.Visibility.Collapsed;
                    this.SetupCompletePageGrid.Visibility = System.Windows.Visibility.Visible;
                }
                else if (this.SetupCompletePageGrid.Visibility == System.Windows.Visibility.Visible)
                {
                    await this.RunAsyncOperation(async () =>
                    {
                        this.StatusMessageTextBlock.Text = "Importing data, this may take a few moments...";
                        await this.FinalizeNewUser();
                        this.Close();
                    });
                }
            });
            this.StatusMessageTextBlock.Text = string.Empty;
        }

        private async void BotLogInButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            bool result = await this.RunAsyncOperation(async () =>
            {
                return await ChannelSession.ConnectBot();
            });

            if (result)
            {
                this.BotLoggedInNameTextBlock.Text = ChannelSession.MixerBotUser.username;
                if (!string.IsNullOrEmpty(ChannelSession.MixerBotUser.avatarUrl))
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    this.BotProfileAvatar.SetUserAvatarUrl(ChannelSession.MixerBotUser.id);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }

                this.BotLogInGrid.Visibility = System.Windows.Visibility.Hidden;
                this.BotLoggedInGrid.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void InstallOBSStudioPlugin_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string pluginPath = Path.Combine(this.directoryPath, "OBS\\obs-websocket-4.3.3-Windows-Installer.exe");
            if (File.Exists(pluginPath))
            {
                ProcessHelper.LaunchProgram(pluginPath);
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

        private async void ConnectToSLOBSButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            bool result = await this.RunAsyncOperation(async () =>
            {
                return await ChannelSession.Services.InitializeStreamlabsOBSService();
            });

            if (result)
            {
                ChannelSession.Settings.EnableStreamlabsOBSConnection = true;
                this.SLOBSConnectedSuccessfulTextBlock.Visibility = System.Windows.Visibility.Visible;
                this.ConnectToSLOBSButton.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                await MessageBoxHelper.ShowMessageDialog("Could not connect to Streamlabs OBS, please make sure Streamlabs OBS is running.");
            }
        }

        private void ScorpBotDirectoryBrowseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string folderPath = ChannelSession.Services.FileService.ShowOpenFolderDialog();
            if (!string.IsNullOrEmpty(folderPath))
            {
                this.ScorpBotDirectoryTextBox.Text = folderPath;
            }
        }

        private void StreamlabsChatBotDataFileBrowseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog("Excel File|*.xlsx");
            if (!string.IsNullOrEmpty(filePath))
            {
                this.StreamlabsChatBotDataFilePathTextBox.Text = filePath;
            }
        }

        private void SoundwaveProfileCheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            SoundwaveProfileItem profile = (SoundwaveProfileItem)checkBox.DataContext;
            this.soundwaveProfiles.First(p => p.Name.Equals(profile.Name)).AddProfile = checkBox.IsChecked.GetValueOrDefault();
        }

        private async Task<ScorpBotData> GatherScorpBotData(string folderPath)
        {
            try
            {
                ScorpBotData scorpBotData = new ScorpBotData();

                string dataPath = Path.Combine(folderPath, "Data");
                string settingsFilePath = Path.Combine(dataPath, "settings.ini");
                if (Directory.Exists(dataPath) && File.Exists(settingsFilePath))
                {
                    IEnumerable<string> lines = File.ReadAllLines(settingsFilePath);

                    string currentGroup = null;
                    foreach (var line in lines)
                    {
                        if (line.Contains("="))
                        {
                            string[] splits = line.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                            if (splits.Count() == 2)
                            {
                                scorpBotData.Settings[currentGroup][splits[0]] = splits[1];
                            }
                        }
                        else
                        {
                            currentGroup = line.Replace("[", "").Replace("]", "").ToLower();
                            scorpBotData.Settings[currentGroup] = new Dictionary<string, string>();
                        }
                    }

                    string databasePath = Path.Combine(dataPath, "Database");
                    if (Directory.Exists(databasePath))
                    {
                        SQLiteDatabaseWrapper databaseWrapper = new SQLiteDatabaseWrapper(databasePath);

                        databaseWrapper.DatabaseFilePath = Path.Combine(databasePath, "CommandsDB.sqlite");
                        await databaseWrapper.RunReadCommand("SELECT * FROM RegCommand",
                        (reader) =>
                        {
                            scorpBotData.Commands.Add(new ScorpBotCommand(reader));
                        });

                        databaseWrapper.DatabaseFilePath = Path.Combine(databasePath, "Timers2DB.sqlite");
                        await databaseWrapper.RunReadCommand("SELECT * FROM TimeCommand",
                        (reader) =>
                        {
                            scorpBotData.Timers.Add(new ScorpBotTimer(reader));
                        });

                        databaseWrapper.DatabaseFilePath = Path.Combine(databasePath, "FilteredWordsDB.sqlite");
                        await databaseWrapper.RunReadCommand("SELECT * FROM Word",
                        (reader) =>
                        {
                            scorpBotData.FilteredWords.Add(((string)reader["word"]).ToLower());
                        });

                        databaseWrapper.DatabaseFilePath = Path.Combine(databasePath, "QuotesDB.sqlite");
                        await databaseWrapper.RunReadCommand("SELECT * FROM Quotes",
                        (reader) =>
                        {
                            scorpBotData.Quotes.Add((string)reader["quote_text"]);
                        });

                        databaseWrapper.DatabaseFilePath = Path.Combine(databasePath, "RankDB.sqlite");
                        await databaseWrapper.RunReadCommand("SELECT * FROM Rank",
                        (reader) =>
                        {
                            scorpBotData.Ranks.Add(new ScorpBotRank(reader));
                        });

                        databaseWrapper.DatabaseFilePath = Path.Combine(databasePath, "Viewers3DB.sqlite");
                        await databaseWrapper.RunReadCommand("SELECT * FROM Viewer",
                        (reader) =>
                        {
                            if (reader["BeamID"] != null && int.TryParse((string)reader["BeamID"], out int id))
                            {
                                scorpBotData.Viewers.Add(new ScorpBotViewer(reader));
                            }
                        });
                    }
                }

                return scorpBotData;
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        private async Task<StreamlabsChatBotData> GatherStreamlabsChatBotSettings(string filePath)
        {
            try
            {
                StreamlabsChatBotData data = new StreamlabsChatBotData();
                await Task.Run(() =>
                {
                    using (NetOffice.ExcelApi.Application application = new NetOffice.ExcelApi.Application())
                    {
                        application.DisplayAlerts = false;

                        NetOffice.ExcelApi.Workbook workbook = application.Workbooks.Open(filePath);
                        if (workbook != null)
                        {
                            foreach (NetOffice.ExcelApi.Worksheet worksheet in workbook.Worksheets.AsEnumerable())
                            {
                                if (worksheet != null)
                                {
                                    List<List<string>> dataValues = new List<List<string>>();

                                    int totalColumns = 0;
                                    bool hasValue = false;
                                    do
                                    {
                                        hasValue = false;
                                        NetOffice.ExcelApi.Range range = worksheet.Cells[1, totalColumns + 1];
                                        if (range.Value != null)
                                        {
                                            totalColumns++;
                                            hasValue = true;
                                        }
                                    } while (hasValue);

                                    int currentRow = 2;
                                    List<string> values = new List<string>();
                                    do
                                    {
                                        values = new List<string>();
                                        for (int i = 1; i <= totalColumns; i++)
                                        {
                                            NetOffice.ExcelApi.Range range = worksheet.Cells[currentRow, i];
                                            if (range.Value != null)
                                            {
                                                values.Add(range.Value.ToString());
                                            }
                                            else
                                            {
                                                values.Add(string.Empty);
                                            }
                                        }

                                        if (!values.All(v => string.IsNullOrEmpty(v)))
                                        {
                                            dataValues.Add(values);
                                        }
                                        currentRow++;
                                    } while (!values.All(v => string.IsNullOrEmpty(v)));

                                    if (worksheet.Name.Equals("Commands"))
                                    {
                                        data.AddCommands(dataValues);
                                    }
                                    else if (worksheet.Name.Equals("Timers"))
                                    {
                                        data.AddTimers(dataValues);
                                    }
                                    else if (worksheet.Name.Equals("Quotes") || worksheet.Name.Equals("Extra Quotes"))
                                    {
                                        data.AddQuotes(dataValues);
                                    }
                                    else if (worksheet.Name.Equals("Ranks"))
                                    {
                                        data.AddRanks(dataValues);
                                    }
                                    else if (worksheet.Name.Equals("Currency"))
                                    {
                                        data.AddViewers(dataValues);
                                    }
                                    else if (worksheet.Name.Equals("Events"))
                                    {
                                        data.AddEvents(dataValues);
                                    }
                                }
                            }
                        }
                        application.Quit();
                    }
                });
                return data;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return null;
        }

        private async Task GatherSoundwaveSettings()
        {
            if (Directory.Exists(SoundwaveInteractiveAppDataSettingsFolder))
            {
                string interactiveFilePath = Path.Combine(SoundwaveInteractiveAppDataSettingsFolder, "interactive.json");
                string profilesFilePath = Path.Combine(SoundwaveInteractiveAppDataSettingsFolder, "profiles.json");
                string soundsFilePath = Path.Combine(SoundwaveInteractiveAppDataSettingsFolder, "sounds.json");
                if (File.Exists(interactiveFilePath) && File.Exists(profilesFilePath) && File.Exists(soundsFilePath))
                {
                    JObject interactive = await SerializerHelper.DeserializeFromFile<JObject>(interactiveFilePath);
                    JObject profiles = await SerializerHelper.DeserializeFromFile<JObject>(profilesFilePath);
                    JObject sounds = await SerializerHelper.DeserializeFromFile<JObject>(soundsFilePath);

                    this.soundwaveData = new SoundwaveSettings();
                    if (interactive != null && profiles != null && sounds != null)
                    {
                        this.soundwaveData.Initialize(interactive, profiles, sounds);
                    }
                }
            }
        }

        private async Task FinalizeNewUser()
        {
            if (this.scorpBotData != null)
            {
                // Import Ranks
                int rankEnabled = this.scorpBotData.GetIntSettingsValue("currency", "enabled");
                string rankName = this.scorpBotData.GetSettingsValue("currency", "name", "Rank");
                int rankInterval = this.scorpBotData.GetIntSettingsValue("currency", "onlinepayinterval");
                int rankAmount = this.scorpBotData.GetIntSettingsValue("currency", "activeuserbonus");
                int rankMaxAmount = this.scorpBotData.GetIntSettingsValue("currency", "maxlimit");
                if (rankMaxAmount <= 0)
                {
                    rankMaxAmount = int.MaxValue;
                }
                int rankOnFollowBonus = this.scorpBotData.GetIntSettingsValue("currency", "onfollowbonus");
                int rankOnSubBonus = this.scorpBotData.GetIntSettingsValue("currency", "onsubbonus");
                int rankSubBonus = this.scorpBotData.GetIntSettingsValue("currency", "subbonus");
                string rankCommand = this.scorpBotData.GetSettingsValue("currency", "command", "");
                string rankCommandResponse = this.scorpBotData.GetSettingsValue("currency", "response", "");
                string rankUpCommand = this.scorpBotData.GetSettingsValue("currency", "Currency1RankUpMsg", "");
                int rankAccumulationType = this.scorpBotData.GetIntSettingsValue("currency", "ranksrectype");

                UserCurrencyViewModel rankCurrency = null;
                UserCurrencyViewModel rankPointsCurrency = null;
                if (!string.IsNullOrEmpty(rankName))
                {
                    if (rankAccumulationType == 1)
                    {
                        rankCurrency = new UserCurrencyViewModel()
                        {
                            Name = rankName.Equals("Points") ? "Hours" : rankName,
                            SpecialIdentifier = SpecialIdentifierStringBuilder.ConvertToSpecialIdentifier(rankName.Equals("Points") ? "Hours" : rankName),
                            AcquireInterval = 60,
                            AcquireAmount = 1,
                            MaxAmount = rankMaxAmount,
                        };

                        if (rankInterval >= 0 && rankAmount >= 0)
                        {
                            rankPointsCurrency = new UserCurrencyViewModel()
                            {
                                Name = "Points",
                                SpecialIdentifier = SpecialIdentifierStringBuilder.ConvertToSpecialIdentifier("points"),
                                AcquireInterval = rankInterval,
                                AcquireAmount = rankAmount,
                                MaxAmount = rankMaxAmount,
                                OnFollowBonus = rankOnFollowBonus,
                                OnSubscribeBonus = rankOnSubBonus,
                                SubscriberBonus = rankSubBonus,
                                ModeratorBonus = rankSubBonus,
                                IsPrimary = true
                            };

                            ChannelSession.Settings.Currencies[rankPointsCurrency.ID] = rankPointsCurrency;
                        }
                    }
                    else if (rankInterval >= 0 && rankAmount >= 0)
                    {
                        rankCurrency = new UserCurrencyViewModel()
                        {
                            Name = rankName,
                            SpecialIdentifier = SpecialIdentifierStringBuilder.ConvertToSpecialIdentifier(rankName),
                            AcquireInterval = rankInterval,
                            AcquireAmount = rankAmount,
                            MaxAmount = rankMaxAmount,
                            OnFollowBonus = rankOnFollowBonus,
                            OnSubscribeBonus = rankOnSubBonus,
                            SubscriberBonus = rankSubBonus,
                            ModeratorBonus = rankSubBonus,
                            IsPrimary = true
                        };
                    }
                }

                // Import Currency
                int currencyEnabled = this.scorpBotData.GetIntSettingsValue("currency2", "enabled");
                string currencyName = this.scorpBotData.GetSettingsValue("currency2", "name", "Currency");
                int currencyInterval = this.scorpBotData.GetIntSettingsValue("currency2", "onlinepayinterval");
                int currencyAmount = this.scorpBotData.GetIntSettingsValue("currency2", "activeuserbonus");
                int currencyMaxAmount = this.scorpBotData.GetIntSettingsValue("currency2", "maxlimit");
                if (currencyMaxAmount <= 0)
                {
                    currencyMaxAmount = int.MaxValue;
                }
                int currencyOnFollowBonus = this.scorpBotData.GetIntSettingsValue("currency2", "onfollowbonus");
                int currencyOnSubBonus = this.scorpBotData.GetIntSettingsValue("currency2", "onsubbonus");
                int currencySubBonus = this.scorpBotData.GetIntSettingsValue("currency2", "subbonus");
                string currencyCommand = this.scorpBotData.GetSettingsValue("currency2", "command", "");
                string currencyCommandResponse = this.scorpBotData.GetSettingsValue("currency2", "response", "");

                UserCurrencyViewModel currency = null;
                if (!string.IsNullOrEmpty(currencyName) && currencyInterval >= 0 && currencyAmount >= 0)
                {
                    currency = new UserCurrencyViewModel()
                    {
                        Name = currencyName,
                        SpecialIdentifier = SpecialIdentifierStringBuilder.ConvertToSpecialIdentifier(currencyName),
                        AcquireInterval = currencyInterval,
                        AcquireAmount = currencyAmount,
                        MaxAmount = currencyMaxAmount,
                        OnFollowBonus = currencyOnFollowBonus,
                        OnSubscribeBonus = currencyOnSubBonus,
                        IsPrimary = true
                    };
                    ChannelSession.Settings.Currencies[currency.ID] = currency;

                    if (!string.IsNullOrEmpty(currencyCommand) && !string.IsNullOrEmpty(currencyCommandResponse))
                    {
                        currencyCommandResponse = currencyCommandResponse.Replace("$points2", "$" + currency.UserAmountSpecialIdentifier);
                        currencyCommandResponse = currencyCommandResponse.Replace("$currencyname2", currency.Name);
                        this.scorpBotData.Commands.Add(new ScorpBotCommand(currencyCommand, currencyCommandResponse));
                    }
                }

                foreach (ScorpBotViewer viewer in this.scorpBotData.Viewers)
                {
                    ChannelSession.Settings.UserData[viewer.ID] = new UserDataViewModel(viewer);

                    if (rankPointsCurrency != null)
                    {
                        ChannelSession.Settings.UserData[viewer.ID].SetCurrencyAmount(rankPointsCurrency, (int)viewer.RankPoints);
                    }

                    if (rankCurrency != null)
                    {
                        ChannelSession.Settings.UserData[viewer.ID].SetCurrencyAmount(rankCurrency, (rankPointsCurrency != null) ? (int)viewer.Hours : (int)viewer.RankPoints);
                    }

                    if (currency != null)
                    {
                        ChannelSession.Settings.UserData[viewer.ID].SetCurrencyAmount(currency, (int)viewer.Currency);
                    }
                }

                if (rankCurrency != null)
                {
                    ChannelSession.Settings.Currencies[rankCurrency.ID] = rankCurrency;

                    foreach (ScorpBotRank rank in this.scorpBotData.Ranks)
                    {
                        rankCurrency.Ranks.Add(new UserRankViewModel(rank.Name, rank.Amount));
                    }

                    if (!string.IsNullOrEmpty(rankCommand) && !string.IsNullOrEmpty(rankCommandResponse))
                    {
                        rankCommandResponse = rankCommandResponse.Replace(" / Raids: $raids", "");
                        rankCommandResponse = rankCommandResponse.Replace("$rank", "$" + rankCurrency.UserRankNameSpecialIdentifier);
                        rankCommandResponse = rankCommandResponse.Replace("$points", "$" + rankCurrency.UserAmountSpecialIdentifier);
                        rankCommandResponse = rankCommandResponse.Replace("$currencyname", rankCurrency.Name);
                        this.scorpBotData.Commands.Add(new ScorpBotCommand(rankCommand, rankCommandResponse));
                    }

                    if (!string.IsNullOrEmpty(rankUpCommand))
                    {
                        rankUpCommand = rankUpCommand.Replace("$rank", "$" + rankCurrency.UserRankNameSpecialIdentifier);
                        rankUpCommand = rankUpCommand.Replace("$points", "$" + rankCurrency.UserAmountSpecialIdentifier);
                        rankUpCommand = rankUpCommand.Replace("$currencyname", rankCurrency.Name);

                        ScorpBotCommand scorpCommand = new ScorpBotCommand("rankup", rankUpCommand);
                        ChatCommand chatCommand = new ChatCommand(scorpCommand);

                        rankCurrency.RankChangedCommand = new CustomCommand("User Rank Changed");
                        rankCurrency.RankChangedCommand.Actions.AddRange(chatCommand.Actions);
                    }
                }

                foreach (ScorpBotCommand command in this.scorpBotData.Commands)
                {
                    command.ProcessData(currency, rankCurrency);
                    ChannelSession.Settings.ChatCommands.Add(new ChatCommand(command));
                }

                foreach (ScorpBotTimer timer in this.scorpBotData.Timers)
                {
                    ChannelSession.Settings.TimerCommands.Add(new TimerCommand(timer));
                }

                foreach (string quote in this.scorpBotData.Quotes)
                {
                    ChannelSession.Settings.UserQuotes.Add(new UserQuoteViewModel(quote));
                }

                if (ChannelSession.Settings.UserQuotes.Count > 0)
                {
                    ChannelSession.Settings.QuotesEnabled = true;
                }

                if (this.scorpBotData.GetBoolSettingsValue("settings", "filtwordsen"))
                {
                    foreach (string filteredWord in this.scorpBotData.FilteredWords)
                    {
                        ChannelSession.Settings.FilteredWords.Add(filteredWord);
                    }
                    ChannelSession.Settings.ModerationFilteredWordsExcempt = this.scorpBotData.GetUserRoleSettingsValue("settings", "FilteredWordsPerm");
                }

                if (this.scorpBotData.GetBoolSettingsValue("settings", "chatcapschecknowarnregs"))
                {
                    ChannelSession.Settings.ModerationChatTextExcempt = MixerRoleEnum.User;
                }
                else if (this.scorpBotData.GetBoolSettingsValue("settings", "chatcapschecknowarnsubs"))
                {
                    ChannelSession.Settings.ModerationChatTextExcempt = MixerRoleEnum.Subscriber;
                }
                else if (this.scorpBotData.GetBoolSettingsValue("settings", "chatcapschecknowarnmods"))
                {
                    ChannelSession.Settings.ModerationChatTextExcempt = MixerRoleEnum.Mod;
                }
                else
                {
                    ChannelSession.Settings.ModerationChatTextExcempt = MixerRoleEnum.Streamer;
                }

                ChannelSession.Settings.ModerationCapsBlockIsPercentage = !this.scorpBotData.GetBoolSettingsValue("settings", "chatcapsfiltertype");
                if (ChannelSession.Settings.ModerationCapsBlockIsPercentage)
                {
                    ChannelSession.Settings.ModerationCapsBlockCount = this.scorpBotData.GetIntSettingsValue("settings", "chatperccaps");
                }
                else
                {
                    ChannelSession.Settings.ModerationCapsBlockCount = this.scorpBotData.GetIntSettingsValue("settings", "chatmincaps");
                }

                ChannelSession.Settings.ModerationBlockLinks = this.scorpBotData.GetBoolSettingsValue("settings", "chatlinkalertsdel");
                ChannelSession.Settings.ModerationBlockLinksExcempt = this.scorpBotData.GetUserRoleSettingsValue("settings", "chatlinkalertsdelperm");
            }

            if (this.streamlabsChatBotData != null)
            {
                UserCurrencyViewModel rank = new UserCurrencyViewModel()
                {
                    Name = "Rank",
                    SpecialIdentifier = SpecialIdentifierStringBuilder.ConvertToSpecialIdentifier("rank"),
                    AcquireInterval = 60,
                    AcquireAmount = 1,
                    IsPrimary = true
                };
                
                foreach (StreamlabsChatBotRank slrank in this.streamlabsChatBotData.Ranks)
                {
                    rank.Ranks.Add(new UserRankViewModel(slrank.Name, slrank.Requirement));
                }

                UserCurrencyViewModel currency = new UserCurrencyViewModel()
                {
                    Name = "Points",
                    SpecialIdentifier = SpecialIdentifierStringBuilder.ConvertToSpecialIdentifier("points"),
                    AcquireInterval = 1,
                    AcquireAmount = 1,
                    IsPrimary = true
                };

                ChannelSession.Settings.Currencies[rank.ID] = rank;
                ChannelSession.Settings.Currencies[currency.ID] = currency;

                this.AddCurrencyRankCommands(rank);
                this.AddCurrencyRankCommands(currency);

                foreach (StreamlabsChatBotViewer viewer in this.streamlabsChatBotData.Viewers)
                {
                    UserModel user = await ChannelSession.MixerStreamerConnection.GetUser(viewer.Name);
                    if (user != null)
                    {
                        viewer.ID = user.id;
                        ChannelSession.Settings.UserData[viewer.ID] = new UserDataViewModel(viewer);
                        ChannelSession.Settings.UserData[viewer.ID].SetCurrencyAmount(rank, viewer.Hours);
                        ChannelSession.Settings.UserData[viewer.ID].SetCurrencyAmount(currency, viewer.Points);
                    }
                }

                foreach (StreamlabsChatBotCommand command in this.streamlabsChatBotData.Commands)
                {
                    command.ProcessData(currency, rank);
                    ChannelSession.Settings.ChatCommands.Add(new ChatCommand(command));
                }

                foreach (StreamlabsChatBotTimer timer in this.streamlabsChatBotData.Timers)
                {
                    StreamlabsChatBotCommand command = new StreamlabsChatBotCommand() { Command = timer.Name, Response = timer.Response, Enabled = timer.Enabled };
                    command.ProcessData(currency, rank);
                    ChannelSession.Settings.ChatCommands.Add(new ChatCommand(command));

                    timer.Actions = command.Actions;

                    ChannelSession.Settings.TimerCommands.Add(new TimerCommand(timer));
                }

                foreach (string quote in this.streamlabsChatBotData.Quotes)
                {
                    ChannelSession.Settings.UserQuotes.Add(new UserQuoteViewModel(quote));
                }

                if (ChannelSession.Settings.UserQuotes.Count > 0)
                {
                    ChannelSession.Settings.QuotesEnabled = true;
                }
            }

            if (this.soundwaveData != null && this.soundwaveProfiles != null && this.soundwaveProfiles.Count(p => p.AddProfile) > 0)
            {
                if (this.soundwaveData.StaticCooldown)
                {
                    ChannelSession.Settings.CooldownGroups[SoundwaveInteractiveCooldownGroupName] = this.soundwaveData.StaticCooldownAmount / 1000;
                }

                MixPlayGameListingModel soundwaveGame = this.interactiveGames.FirstOrDefault(g => g.name.Equals(SoundwaveInteractiveGameName));
                if (soundwaveGame != null)
                {
                    MixPlayGameVersionModel version = soundwaveGame.versions.FirstOrDefault();
                    if (version != null)
                    {
                        MixPlayGameVersionModel soundwaveGameVersion = await ChannelSession.MixerStreamerConnection.GetMixPlayGameVersion(version);
                        if (soundwaveGameVersion != null)
                        {
                            MixPlaySceneModel soundwaveGameScene = soundwaveGameVersion.controls.scenes.FirstOrDefault();
                            if (soundwaveGameScene != null)
                            {
                                foreach (string profile in this.soundwaveProfiles.Where(p => p.AddProfile).Select(p => p.Name))
                                {
                                    // Add code logic to create Interactive Game on Mixer that is a copy of the Soundwave Interactive game, but with buttons filed in with name and not disabled
                                    MixPlaySceneModel profileScene = MixPlayGameHelper.CreateDefaultScene();
                                    MixPlayGameListingModel profileGame = await ChannelSession.MixerStreamerConnection.CreateMixPlayGame(ChannelSession.MixerChannel, ChannelSession.MixerStreamerUser, profile, profileScene);
                                    MixPlayGameVersionModel gameVersion = profileGame.versions.FirstOrDefault();
                                    if (gameVersion != null)
                                    {
                                        MixPlayGameVersionModel profileGameVersion = await ChannelSession.MixerStreamerConnection.GetMixPlayGameVersion(gameVersion);
                                        if (profileGameVersion != null)
                                        {
                                            profileScene = profileGameVersion.controls.scenes.First();

                                            for (int i = 0; i < this.soundwaveData.Profiles[profile].Count(); i++)
                                            {
                                                SoundwaveButton soundwaveButton = this.soundwaveData.Profiles[profile][i];
                                                MixPlayButtonControlModel soundwaveControl = (MixPlayButtonControlModel)soundwaveGameScene.allControls.FirstOrDefault(c => c.controlID.Equals(i.ToString()));

                                                MixPlayButtonControlModel button = MixPlayGameHelper.CreateButton(soundwaveButton.name, soundwaveButton.name, soundwaveButton.sparks);
                                                button.position = soundwaveControl.position;

                                                RequirementViewModel requirements = new RequirementViewModel();
                                                requirements.Cooldown.Amount = soundwaveButton.cooldown;
                                                if (this.soundwaveData.StaticCooldown)
                                                {
                                                    requirements.Cooldown.Type = CooldownTypeEnum.Group;
                                                    requirements.Cooldown.GroupName = SoundwaveInteractiveCooldownGroupName;
                                                }
                                                InteractiveButtonCommand command = new InteractiveButtonCommand(profileGame, profileScene, button, InteractiveButtonCommandTriggerType.MouseKeyDown, requirements);

                                                SoundAction action = new SoundAction(soundwaveButton.path, soundwaveButton.volume);
                                                command.Actions.Add(action);

                                                ChannelSession.Settings.InteractiveCommands.Add(command);
                                                profileScene.buttons.Add(button);
                                            }

                                            await ChannelSession.MixerStreamerConnection.UpdateMixPlayGameVersion(profileGameVersion);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            await ChannelSession.SaveSettings();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                ProcessHelper.LaunchLink(e.Uri.AbsoluteUri);
                e.Handled = true;
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        private void AddCurrencyRankCommands(UserCurrencyViewModel currency)
        {
            ChatCommand statusCommand = new ChatCommand("User " + currency.Name, currency.SpecialIdentifier, new RequirementViewModel(MixerRoleEnum.User, 5));
            string statusChatText = string.Empty;
            if (currency.IsRank)
            {
                statusChatText = string.Format("@$username is a ${0} with ${1} {2}!", currency.UserRankNameSpecialIdentifier, currency.UserAmountSpecialIdentifier, currency.Name);
            }
            else
            {
                statusChatText = string.Format("@$username has ${0} {1}!", currency.UserAmountSpecialIdentifier, currency.Name);
            }
            statusCommand.Actions.Add(new ChatAction(statusChatText));
            ChannelSession.Settings.ChatCommands.Add(statusCommand);

            ChatCommand addCommand = new ChatCommand("Add " + currency.Name, "add" + currency.SpecialIdentifier, new RequirementViewModel(MixerRoleEnum.Mod, 5));
            addCommand.Actions.Add(new CurrencyAction(currency, CurrencyActionTypeEnum.AddToSpecificUser, "$arg2text", username: "$targetusername"));
            addCommand.Actions.Add(new ChatAction(string.Format("@$targetusername received $arg2text {0}!", currency.Name)));
            ChannelSession.Settings.ChatCommands.Add(addCommand);

            ChatCommand addAllCommand = new ChatCommand("Add All " + currency.Name, "addall" + currency.SpecialIdentifier, new RequirementViewModel(MixerRoleEnum.Mod, 5));
            addAllCommand.Actions.Add(new CurrencyAction(currency, CurrencyActionTypeEnum.AddToAllChatUsers, "$arg1text"));
            addAllCommand.Actions.Add(new ChatAction(string.Format("Everyone got $arg1text {0}!", currency.Name)));
            ChannelSession.Settings.ChatCommands.Add(addAllCommand);

            if (!currency.IsRank)
            {
                ChatCommand giveCommand = new ChatCommand("Give " + currency.Name, "give" + currency.SpecialIdentifier, new RequirementViewModel(MixerRoleEnum.User, 5));
                giveCommand.Actions.Add(new CurrencyAction(currency, CurrencyActionTypeEnum.AddToSpecificUser, "$arg2text", username: "$targetusername", deductFromUser: true));
                giveCommand.Actions.Add(new ChatAction(string.Format("@$username gave @$targetusername $arg2text {0}!", currency.Name)));
                ChannelSession.Settings.ChatCommands.Add(giveCommand);
            }
        }
    }
}
