using MixItUp.WPF.Windows;
using System.Windows;
using System.Threading.Tasks;
using AutoUpdaterDotNET;
using System;

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

        protected override Task OnLoaded()
        {
            this.NewVersionTextBlock.Text = updateArgs.CurrentVersion.ToString();
            this.UpdateChangelogWebBrowser.Source = new Uri(updateArgs.ChangelogURL);

            return base.OnLoaded();
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
