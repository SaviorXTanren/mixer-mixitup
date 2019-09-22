using MixItUp.Base.Model.API;
using MixItUp.Base.Util;
using MixItUp.WPF.Windows;
using StreamingClient.Base.Util;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF
{
    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : LoadingWindowBase
    {
        private MixItUpUpdateModel update;

        public UpdateWindow(MixItUpUpdateModel update)
        {
            this.update = update;

            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            this.NewVersionTextBlock.Text = this.update.Version.ToString();
            this.CurrentVersionTextBlock.Text = Assembly.GetEntryAssembly().GetName().Version.ToString();

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string changelogHTML = await client.GetStringAsync(this.update.ChangelogLink);
                    this.UpdateChangelogWebBrowser.NavigateToString(changelogHTML);
                }
            }
            catch (Exception ex) { Logger.Log(ex); }

            await base.OnLoaded();
        }

        private async void DownloadUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                string setupFilePath = Path.Combine(Path.GetTempPath(), "MixItUp-Setup.exe");

                bool downloadComplete = false;

                WebClient client = new WebClient();
                client.DownloadFileCompleted += (s, ce) =>
                {
                    downloadComplete = true;
                };

                client.DownloadFileAsync(new Uri("https://github.com/SaviorXTanren/mixer-mixitup/releases/download/Installer-0.3.0/MixItUp-Setup.exe"), setupFilePath);

                while (!downloadComplete)
                {
                    await Task.Delay(1000);
                }
                client.Dispose();

                if (File.Exists(setupFilePath))
                {
                    ProcessHelper.LaunchProgram(setupFilePath, string.Format("\"{0}\"", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)));
                    Application.Current.Shutdown();
                }
            });
        }

        private void SkipUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
