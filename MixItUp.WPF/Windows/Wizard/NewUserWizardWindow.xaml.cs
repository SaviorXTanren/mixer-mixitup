using MixItUp.Base;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.ScorpBot;
using MixItUp.WPF.Util;
using System;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;

namespace MixItUp.WPF.Windows.Wizard
{
    /// <summary>
    /// Interaction logic for NewUserWizardWindow.xaml
    /// </summary>
    public partial class NewUserWizardWindow : LoadingWindowBase
    {
        private ScorpBotData scorpBotData;

        public NewUserWizardWindow()
        {
            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override Task OnLoaded()
        {
            this.IntroPageGrid.Visibility = System.Windows.Visibility.Visible;
            this.BackButton.IsEnabled = false;
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
                    this.scorpBotData = await this.ImportScorpBotData(this.ScorpBotDirectoryTextBox.Text);
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

        private async Task<ScorpBotData> ImportScorpBotData(string folderPath)
        {
            return await this.RunAsyncOperation(async () =>
            {
                try
                {
                    string dataPath = Path.Combine(folderPath, "Data\\Database");
                    if (Directory.Exists(dataPath))
                    {
                        string viewersDB = Path.Combine(dataPath, "Viewers3DB.sqlite");
                        if (File.Exists(viewersDB))
                        {
                            ScorpBotData scorpBotData = new ScorpBotData();
                            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + viewersDB))
                            {
                                await connection.OpenAsync();
                                using (SQLiteCommand command = new SQLiteCommand("SELECT * FROM Viewer", connection))
                                {
                                    using (SQLiteDataReader reader = command.ExecuteReader())
                                    {
                                        while (reader.Read())
                                        {
                                            try
                                            {
                                                scorpBotData.Viewers.Add(new ScorpBotViewer(reader));
                                            }
                                            catch (Exception ex) { Logger.Log(ex); }
                                        }
                                    }
                                }
                            }
                            return scorpBotData;
                        }
                    }
                }
                catch (Exception ex) { Logger.Log(ex); }
                return null;
            });
        }
    }
}
