using MixItUp.WPF.Windows;
using System.Windows;
using System.Threading.Tasks;
using AutoUpdaterDotNET;
using System;
using System.Net.Http;
using MixItUp.Base.Util;

namespace MixItUp.WPF
{
    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : LoadingWindowBase
    {
        private UpdateInfoEventArgs updateArgs;

        public UpdateWindow(UpdateInfoEventArgs updateArgs)
        {
            this.updateArgs = updateArgs;

            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            this.NewVersionTextBlock.Text = updateArgs.CurrentVersion.ToString();

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string changelogHTML = await client.GetStringAsync(updateArgs.ChangelogURL);
                    this.UpdateChangelogWebBrowser.NavigateToString(changelogHTML);
                }
            }
            catch (Exception ex) { Logger.Log(ex); }

            await base.OnLoaded();
        }

        private void DownloadUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (AutoUpdater.DownloadUpdate())
            {
                Application.Current.Shutdown();
            }
        }

        private void SkipUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
