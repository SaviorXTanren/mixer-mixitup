using MixItUp.Base;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Store;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel;
using MixItUp.Base.ViewModel.CommunityCommands;
using MixItUp.WPF.Controls.Dialogs;
using MixItUp.WPF.Controls.MainControls;
using MixItUp.WPF.Windows;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace MixItUp.WPF
{
    /// <summary>
    /// Interaction logic for StreamerWindow.xaml
    /// </summary>
    public partial class MainWindow : LoadingWindowBase
    {
        public class MainWindowUIViewModel : MainWindowViewModel
        {
            public Visibility HelpLinkVisibility { get { return Visibility.Collapsed; } }
        }

        private bool restartApplication = false;

        private bool shutdownStarted = false;
        private bool shutdownComplete = false;

        private MainWindowViewModel viewModel;

        public MainWindow()
            : base(new MainWindowUIViewModel())
        {
            InitializeComponent();

            ChannelSession.OnRestartRequested += ChannelSession_OnRestartRequested;

            this.Closing += MainWindow_Closing;
            this.Initialize(this.StatusBar);

            this.viewModel = (MainWindowViewModel)this.ViewModel;
            this.viewModel.StartLoadingOperationOccurred += (sender, args) =>
            {
                this.StartLoadingOperation();
            };
            this.viewModel.EndLoadingOperationOccurred += (sender, args) =>
            {
                this.EndLoadingOperation();
            };

            if (ChannelSession.AppSettings.Width > 0 && !ChannelSession.AppSettings.DontSaveLastWindowPosition)
            {
                this.WindowStartupLocation = WindowStartupLocation.Manual;
                this.Height = ChannelSession.AppSettings.Height;
                this.Width = ChannelSession.AppSettings.Width;
                this.Top = ChannelSession.AppSettings.Top;
                this.Left = ChannelSession.AppSettings.Left;

                var rect = new System.Drawing.Rectangle((int)this.Left, (int)this.Top, (int)this.Width, (int)this.Height);
                var screen = System.Windows.Forms.Screen.FromRectangle(rect);
                if (!screen.Bounds.Contains(rect))
                {
                    // Off the bottom of the screen?
                    if (this.Top + this.Height > screen.Bounds.Top + screen.Bounds.Height)
                    {
                        this.Top = screen.Bounds.Top + screen.Bounds.Height - this.Height;
                    }

                    // Off the right side of the screen?
                    if (this.Left + this.Width > screen.Bounds.Left + screen.Bounds.Width)
                    {
                        this.Left = screen.Bounds.Left + screen.Bounds.Width - this.Width;
                    }

                    // Off the top of the screen?
                    if (this.Top < screen.Bounds.Top)
                    {
                        this.Top = screen.Bounds.Top;
                    }

                    // Off the left side of the screen?
                    if (this.Left < screen.Bounds.Left)
                    {
                        this.Left = screen.Bounds.Left;
                    }
                }

                if (ChannelSession.AppSettings.IsMaximized)
                {
                    WindowState = WindowState.Maximized;
                }
            }
        }

        public void Restart()
        {
            this.restartApplication = true;
            this.Close();
        }

        protected override async Task OnLoaded()
        {
            ServiceManager.Get<IInputService>().Initialize(new WindowInteropHelper(this).Handle);
            foreach (HotKeyConfiguration hotKeyConfiguration in ChannelSession.Settings.HotKeys.Values)
            {
                CommandModelBase command = ChannelSession.Settings.GetCommand(hotKeyConfiguration.CommandID);
                if (command != null)
                {
                    ServiceManager.Get<IInputService>().RegisterHotKey(hotKeyConfiguration.Modifiers, hotKeyConfiguration.VirtualKey);
                }
            }

            if (!string.IsNullOrEmpty(ChannelSession.Settings.Name))
            {
                this.Title += " - " + ChannelSession.Settings.Name;
            }

            this.Title += " - v" + Assembly.GetEntryAssembly().GetName().Version.ToString();

            await this.MainMenu.Initialize(this);

            await this.MainMenu.AddMenuItem(MixItUp.Base.Resources.MixItUpOnline, new MixItUpOnlineControl(), "https://online.mixitupapp.com/alpha");
            await this.MainMenu.AddMenuItem(MixItUp.Base.Resources.Channel, new ChannelControl(), "https://wiki.mixitupapp.com/channel");
            await this.MainMenu.AddMenuItem(MixItUp.Base.Resources.Chat, new ChatControl(), "https://wiki.mixitupapp.com/chat");
            await this.MainMenu.AddMenuItem(MixItUp.Base.Resources.Commands, new ChatCommandsControl(), "https://wiki.mixitupapp.com/commands/chat-commands");
            await this.MainMenu.AddMenuItem(MixItUp.Base.Resources.Events, new EventsControl(), "https://wiki.mixitupapp.com/commands/event-commands");
            await this.MainMenu.AddMenuItem(MixItUp.Base.Resources.Timers, new TimerControl(), "https://wiki.mixitupapp.com/commands/timer-commands");
            await this.MainMenu.AddMenuItem(MixItUp.Base.Resources.ActionGroups, new ActionGroupControl(), "https://wiki.mixitupapp.com/commands/action-groups");
            await this.MainMenu.AddMenuItem(MixItUp.Base.Resources.CommunityCommands, new CommunityCommandsControl(), "https://wiki.mixitupapp.com/commands/community-commands");
            await this.MainMenu.AddMenuItem(MixItUp.Base.Resources.Users, new UsersControl(), "https://wiki.mixitupapp.com/users");
            await this.MainMenu.AddMenuItem(MixItUp.Base.Resources.MusicPlayer, new MusicPlayerControl(), "https://wiki.mixitupapp.com/music-player");
            //await this.MainMenu.AddMenuItem(MixItUp.Base.Resources.Statistics, new StatisticsControl(), "https://wiki.mixitupapp.com/statistics");
            await this.MainMenu.AddMenuItem(MixItUp.Base.Resources.CurrencyRankInventory, new CurrencyRankInventoryControl(), "https://wiki.mixitupapp.com/consumables");
            await this.MainMenu.AddMenuItem(MixItUp.Base.Resources.TwitchChannelPoints, new TwitchChannelPointsControl(), "https://wiki.mixitupapp.com/commands/twitch-channel-point-commands");
            await this.MainMenu.AddMenuItem(MixItUp.Base.Resources.TwitchBits, new TwitchBitsControl(), "https://wiki.mixitupapp.com/commands/twitch-bits-commands");
            await this.MainMenu.AddMenuItem(MixItUp.Base.Resources.TrovoSpells, new TrovoSpellsControl(), "https://wiki.mixitupapp.com/commands/trovo-spell-commands");
            await this.MainMenu.AddMenuItem(MixItUp.Base.Resources.StreamlootsCards, new StreamlootsCardsControl(), "https://wiki.mixitupapp.com/commands/streamloots-card-commands");
            await this.MainMenu.AddMenuItem(MixItUp.Base.Resources.CrowdControl, new CrowdControlControl(), "https://wiki.mixitupapp.com/commands/crowd-control-commands");
            await this.MainMenu.AddMenuItem(MixItUp.Base.Resources.StreamPass, new StreamPassControl(), "https://wiki.mixitupapp.com/consumables/stream-pass");
            await this.MainMenu.AddMenuItem(MixItUp.Base.Resources.RedemptionStore, new RedemptionStoreControl(), "https://wiki.mixitupapp.com/redemption-store");
            await this.MainMenu.AddMenuItem(MixItUp.Base.Resources.OverlayWidgets, new OverlayWidgetsControl(), "https://wiki.mixitupapp.com/overlay-widgets");
            await this.MainMenu.AddMenuItem(MixItUp.Base.Resources.Games, new GamesControl(), "https://wiki.mixitupapp.com/commands/game-commands");
            await this.MainMenu.AddMenuItem(MixItUp.Base.Resources.Giveaway, new GiveawayControl(), "https://wiki.mixitupapp.com/giveaways");
            await this.MainMenu.AddMenuItem(MixItUp.Base.Resources.GameQueue, new GameQueueControl(), "https://wiki.mixitupapp.com/game-queue");
            await this.MainMenu.AddMenuItem(MixItUp.Base.Resources.Quotes, new QuoteControl(), "https://wiki.mixitupapp.com/quotes");
            await this.MainMenu.AddMenuItem(MixItUp.Base.Resources.Moderation, new ModerationControl(), "https://wiki.mixitupapp.com/moderation");
            await this.MainMenu.AddMenuItem(MixItUp.Base.Resources.CommandHistory, new CommandHistoryControl(), "https://wiki.mixitupapp.com/commands/command-history");
            await this.MainMenu.AddMenuItem(MixItUp.Base.Resources.Services, new ServicesControl(), "https://wiki.mixitupapp.com/services");
            await this.MainMenu.AddMenuItem(MixItUp.Base.Resources.Webhooks, new WebhooksControl(), "https://wiki.mixitupapp.com/commands/webhook-commands");
            await this.MainMenu.AddMenuItem(MixItUp.Base.Resources.Accounts, new AccountsControl(), "https://wiki.mixitupapp.com/accounts");
            await this.MainMenu.AddMenuItem(MixItUp.Base.Resources.Changelog, new ChangelogControl(), "https://wiki.mixitupapp.com/");
            await this.MainMenu.AddMenuItem(MixItUp.Base.Resources.About, new AboutControl(), "https://wiki.mixitupapp.com/");

            if (ChannelSession.IsDebug())
            {
                await this.MainMenu.AddMenuItem(MixItUp.Base.Resources.Debug, new DebugControl(), "https://wiki.mixitupapp.com/");
            }

            this.MainMenu.MenuItemSelected(MixItUp.Base.Resources.Chat);

            ActivationProtocolHandler.OnCommunityCommandActivation += ActivationProtocolHandler_OnCommunityCommandActivation;
            ActivationProtocolHandler.OnCommandFileActivation += ActivationProtocolHandler_OnCommandFileActivation;

            if (SettingsV3Upgrader.OverlayV3UpgradeOccurred && ChannelSession.Settings.OverlayEndpointsV3.Count > 1)
            {
                await DialogHelper.ShowCustom(new OverlayEndpointsUpdateDialogControl());
            }
        }

        private async Task StartShutdownProcess()
        {
            try
            {
                Logger.Log(LogLevel.Debug, "Starting shutdown process");

                if (WindowState == WindowState.Maximized)
                {
                    // Use the RestoreBounds as the current values will be 0, 0 and the size of the screen
                    ChannelSession.AppSettings.Top = RestoreBounds.Top;
                    ChannelSession.AppSettings.Left = RestoreBounds.Left;
                    ChannelSession.AppSettings.Height = RestoreBounds.Height;
                    ChannelSession.AppSettings.Width = RestoreBounds.Width;
                    ChannelSession.AppSettings.IsMaximized = true;
                }
                else
                {
                    ChannelSession.AppSettings.Top = this.Top;
                    ChannelSession.AppSettings.Left = this.Left;
                    ChannelSession.AppSettings.Height = this.Height;
                    ChannelSession.AppSettings.Width = this.Width;
                    ChannelSession.AppSettings.IsMaximized = false;
                }

                Properties.Settings.Default.Save();

                this.ShuttingDownGrid.Visibility = Visibility.Visible;
                this.MainMenu.Visibility = Visibility.Collapsed;

                await ServiceManager.Get<SettingsService>().Save(ChannelSession.Settings);

                await ChannelSession.AppSettings.Save();

                await ChannelSession.Close();

                this.shutdownComplete = true;

                Logger.Log(LogLevel.Debug, "Shutdown process complete");
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            this.Close();
            if (this.restartApplication)
            {
                ServiceManager.Get<IProcessService>().LaunchProgram(Application.ResourceAssembly.Location);
            }
        }

        private async void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Activate();
            if (!this.shutdownStarted)
            {
                e.Cancel = true;
                if (await DialogHelper.ShowConfirmation(MixItUp.Base.Resources.ExitConfirmation))
                {
                    this.shutdownStarted = true;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    this.StartShutdownProcess();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
            }
            else if (!this.shutdownComplete)
            {
                e.Cancel = true;
            }
        }

        private void ChannelSession_OnRestartRequested(object sender, EventArgs e) { this.Restart(); }

        private async void ActivationProtocolHandler_OnCommunityCommandActivation(object sender, Guid commandID)
        {
            await DispatcherHelper.Dispatcher.InvokeAsync(async () =>
            {
                CommunityCommandDetailsModel commandDetails = await ServiceManager.Get<MixItUpService>().GetCommandDetails(commandID);
                if (commandDetails != null)
                {
                    await CommunityCommandsControl.ProcessDownloadedCommunityCommand(new CommunityCommandDetailsViewModel(commandDetails));
                }
            });
        }

        private async void ActivationProtocolHandler_OnCommandFileActivation(object sender, CommandModelBase command)
        {
            await DispatcherHelper.Dispatcher.InvokeAsync(async () =>
            {
                await DialogHelper.ShowCustom(new CommandImporterDialogControl(command));
            });
        }
    }
}
