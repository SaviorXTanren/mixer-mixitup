using Mixer.Base.Model.MixPlay;
using MixItUp.Base;
using MixItUp.Base.Model.MixPlay;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Settings
{
    /// <summary>
    /// Interaction logic for InteractiveSettingsControl.xaml
    /// </summary>
    public partial class InteractiveSettingsControl : SettingsControlBase
    {
        private static readonly MixPlayGameModel NoneInteractiveGame = new MixPlayGameModel() { id = 0, name = "NONE" };

        private ObservableCollection<MixPlaySharedProjectModel> customInteractiveProjects = new ObservableCollection<MixPlaySharedProjectModel>();

        public InteractiveSettingsControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            List<MixPlayGameModel> interactiveGames = new List<MixPlayGameModel>();
            interactiveGames.Add(InteractiveSettingsControl.NoneInteractiveGame);
            interactiveGames.AddRange(await ChannelSession.Services.MixPlay.GetAllGames());
            this.DefaultInteractiveGameComboBox.ItemsSource = interactiveGames;

            MixPlayGameModel game = interactiveGames.FirstOrDefault(g => g.id.Equals(ChannelSession.Settings.DefaultMixPlayGame));
            if (game == null) { game = InteractiveSettingsControl.NoneInteractiveGame; }
            this.DefaultInteractiveGameComboBox.SelectedItem = game;

            this.PreventUnknownInteractiveUsersToggleButton.IsChecked = ChannelSession.Settings.PreventUnknownMixPlayUsers;

            this.CustomInteractiveProjectsListView.ItemsSource = this.customInteractiveProjects;
            this.customInteractiveProjects.Clear();
            foreach (MixPlaySharedProjectModel sharedProject in ChannelSession.Settings.CustomMixPlayProjectIDs)
            {
                this.customInteractiveProjects.Add(sharedProject);
            }

            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.InitializeInternal();
        }

        private void DefaultInteractiveGameComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.DefaultInteractiveGameComboBox.SelectedIndex >= 0)
            {
                MixPlayGameModel game = (MixPlayGameModel)this.DefaultInteractiveGameComboBox.SelectedItem;
                ChannelSession.Settings.DefaultMixPlayGame = game.id;
            }
        }

        private void PreventUnknownInteractiveUsersToggleButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            ChannelSession.Settings.PreventUnknownMixPlayUsers = this.PreventUnknownInteractiveUsersToggleButton.IsChecked.GetValueOrDefault();
        }

        private void PreventSmallerCooldownsToggleButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            ChannelSession.Settings.PreventSmallerMixPlayCooldowns = this.PreventSmallerCooldownsToggleButton.IsChecked.GetValueOrDefault();
        }

        private async void AddCustomInteractiveProjectButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(() =>
            {
                if (!string.IsNullOrEmpty(this.CustomInteractiveProjectVersionIDTextBox.Text) && !string.IsNullOrEmpty(this.CustomInteractiveProjectShareCodeTextBox.Text))
                {
                    if (uint.TryParse(this.CustomInteractiveProjectVersionIDTextBox.Text, out uint versionID) && !ChannelSession.Settings.CustomMixPlayProjectIDs.Any(p => p.VersionID == versionID))
                    {
                        MixPlaySharedProjectModel project = new MixPlaySharedProjectModel(versionID, this.CustomInteractiveProjectShareCodeTextBox.Text);
                        ChannelSession.Settings.CustomMixPlayProjectIDs.Add(project);
                        this.customInteractiveProjects.Add(project);

                        GlobalEvents.InteractiveSharedProjectAdded(project);
                    }
                }
                this.CustomInteractiveProjectVersionIDTextBox.Text = string.Empty;
                this.CustomInteractiveProjectShareCodeTextBox.Text = string.Empty;

                return Task.FromResult(0);
            });
        }

        private void DeleteCustomInteractiveProjectButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button button = (Button)sender;
            MixPlaySharedProjectModel project = (MixPlaySharedProjectModel)button.DataContext;
            ChannelSession.Settings.CustomMixPlayProjectIDs.Remove(project);
            this.customInteractiveProjects.Remove(project);
        }
    }
}
