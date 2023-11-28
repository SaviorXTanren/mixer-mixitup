using MixItUp.Base;
using MixItUp.Base.Model.API;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.WPF.Windows;
using MixItUp.WPF.Windows.Wizard;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace MixItUp.WPF
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : LoadingWindowBase
    {
        private MixItUpUpdateModel currentUpdate;
        private bool updateFound = false;

        private ThreadSafeObservableCollection<SettingsV3Model> streamerSettings = new ThreadSafeObservableCollection<SettingsV3Model>();

        public LoginWindow()
        {
            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            ChannelSession.OnRestartRequested += ChannelSession_OnRestartRequested;

            this.Title += " - v" + Assembly.GetEntryAssembly().GetName().Version.ToString();

            if (ServiceManager.Get<IProcessService>().GetProcessesByName("MixItUp").Count() > 1)
            {
                if (!await DialogHelper.ShowConfirmation(MixItUp.Base.Resources.MixItUpIsAlreadyRunning))
                {
                    this.Close();
                }
            }

            this.ExistingStreamerComboBox.ItemsSource = streamerSettings;

            await this.CheckForUpdates();

            foreach (SettingsV3Model setting in (await ServiceManager.Get<SettingsService>().GetAllSettings()).OrderBy(s => s.Name))
            {
                this.streamerSettings.Add(setting);
            }

            if (this.streamerSettings.Count > 0)
            {
                this.ExistingStreamerComboBox.Visibility = Visibility.Visible;
                this.StreamerLoginButton.IsEnabled = true;
                if (this.streamerSettings.Count == 1)
                {
                    this.ExistingStreamerComboBox.SelectedIndex = 0;
                }
            }

            if (ChannelSession.AppSettings.AutoLogInID != Guid.Empty)
            {
                var allSettings = this.streamerSettings.ToList();
                SettingsV3Model autoLogInSettings = allSettings.FirstOrDefault(s => s.ID == ChannelSession.AppSettings.AutoLogInID);
                if (autoLogInSettings != null)
                {
                    await Task.Delay(5000);

                    if (!updateFound)
                    {
                        if (await this.ExistingSettingLogin(autoLogInSettings))
                        {
                            LoadingWindowBase newWindow = null;
                            if (ChannelSession.Settings.ReRunWizard)
                            {
                                newWindow = new NewUserWizardWindow();
                            }
                            else
                            {
                                newWindow = new MainWindow();
                            }
                            ShowMainWindow(newWindow);
                            this.Hide();
                            this.Close();
                            return;
                        }
                    }
                }
            }

            await base.OnLoaded();
        }

        private async void StreamerLoginButton_Click(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                if (this.ExistingStreamerComboBox.SelectedIndex >= 0)
                {
                    SettingsV3Model setting = (SettingsV3Model)this.ExistingStreamerComboBox.SelectedItem;
                    if (setting.ID != Guid.Empty)
                    {
                        if (await this.ExistingSettingLogin(setting))
                        {
                            LoadingWindowBase newWindow = null;
                            if (ChannelSession.Settings.ReRunWizard)
                            {
                                newWindow = new NewUserWizardWindow();
                            }
                            else
                            {
                                newWindow = new MainWindow();
                            }

                            ChannelSession.OnRestartRequested -= ChannelSession_OnRestartRequested;

                            ShowMainWindow(newWindow);
                            this.Hide();
                            this.Close();
                            return;
                        }
                    }
                }
                else
                {
                    await DialogHelper.ShowMessage(MixItUp.Base.Resources.LoginErrorNoStreamerAccount);
                }
            });
        }

        private async Task CheckForUpdates()
        {
            this.currentUpdate = await ServiceManager.Get<MixItUpService>().GetLatestUpdate();
            if (this.currentUpdate != null && this.currentUpdate.SystemVersion > Assembly.GetEntryAssembly().GetName().Version)
            {
                updateFound = true;
                UpdateWindow window = new UpdateWindow(this.currentUpdate);
                window.Show();
            }
        }

        private async Task<bool> ExistingSettingLogin(SettingsV3Model setting)
        {
            Result result = await ChannelSession.Connect(setting);
            if (result.Success)
            {
                result = await ChannelSession.InitializeSession();
                if (result.Success)
                {
                    return true;
                }
            }
            await DialogHelper.ShowMessage(result.Message);
            return false;
        }


        private async void NewStreamerLoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (await this.ShowLicenseAgreement())
            {
                ChannelSession.OnRestartRequested -= ChannelSession_OnRestartRequested;

                ShowMainWindow(new NewUserWizardWindow());
                this.Hide();
                this.Close();
            }
        }

        private async void RestoreBackupButton_Click(object sender, RoutedEventArgs e)
        {
            await SettingsV3Model.RestoreSettingsBackup();
        }

        private async void ChannelSession_OnRestartRequested(object sender, EventArgs e)
        {
            await ChannelSession.AppSettings.Save();

            this.Close();

            ServiceManager.Get<IProcessService>().LaunchProgram(Application.ResourceAssembly.Location);
        }

        private Task<bool> ShowLicenseAgreement()
        {
            LicenseAgreementWindow window = new LicenseAgreementWindow();
            TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();
            window.Owner = Application.Current.MainWindow;
            window.Closed += (s, a) => task.SetResult(window.Accepted);
            window.Show();
            window.Focus();
            return task.Task;
        }

        public void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            ServiceManager.Get<IProcessService>().LaunchLink(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}