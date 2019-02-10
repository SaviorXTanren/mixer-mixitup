using Mixer.Base.Model.Interactive;
using MixItUp.Base;
using MixItUp.Base.Model.Interactive;
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
        private static readonly InteractiveGameModel NoneInteractiveGame = new InteractiveGameModel() { id = 0, name = "NONE" };

        private ObservableCollection<InteractiveSharedProjectModel> customInteractiveProjects = new ObservableCollection<InteractiveSharedProjectModel>();

        public InteractiveSettingsControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            List<InteractiveGameModel> interactiveGames = new List<InteractiveGameModel>();
            interactiveGames.Add(InteractiveSettingsControl.NoneInteractiveGame);
            interactiveGames.AddRange(await ChannelSession.Interactive.GetAllConnectableGames());
            this.DefaultInteractiveGameComboBox.ItemsSource = interactiveGames;

            InteractiveGameModel game = interactiveGames.FirstOrDefault(g => g.id.Equals(ChannelSession.Settings.DefaultInteractiveGame));
            if (game == null) { game = InteractiveSettingsControl.NoneInteractiveGame; }
            this.DefaultInteractiveGameComboBox.SelectedItem = game;

            this.PreventUnknownInteractiveUsersToggleButton.IsChecked = ChannelSession.Settings.PreventUnknownInteractiveUsers;

            this.CustomInteractiveProjectsListView.ItemsSource = this.customInteractiveProjects;
            this.customInteractiveProjects.Clear();
            foreach (InteractiveSharedProjectModel sharedProject in ChannelSession.Settings.CustomInteractiveProjectIDs)
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
                InteractiveGameModel game = (InteractiveGameModel)this.DefaultInteractiveGameComboBox.SelectedItem;
                ChannelSession.Settings.DefaultInteractiveGame = game.id;
            }
        }

        private void PreventUnknownInteractiveUsersToggleButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            ChannelSession.Settings.PreventUnknownInteractiveUsers = this.PreventUnknownInteractiveUsersToggleButton.IsChecked.GetValueOrDefault();
        }

        private void PreventSmallerCooldownsToggleButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            ChannelSession.Settings.PreventSmallerCooldowns = this.PreventSmallerCooldownsToggleButton.IsChecked.GetValueOrDefault();
        }

        private async void AddCustomInteractiveProjectButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(() =>
            {
                if (!string.IsNullOrEmpty(this.CustomInteractiveProjectVersionIDTextBox.Text) && !string.IsNullOrEmpty(this.CustomInteractiveProjectShareCodeTextBox.Text))
                {
                    if (uint.TryParse(this.CustomInteractiveProjectVersionIDTextBox.Text, out uint versionID) && !ChannelSession.Settings.CustomInteractiveProjectIDs.Any(p => p.VersionID == versionID))
                    {
                        InteractiveSharedProjectModel project = new InteractiveSharedProjectModel(versionID, this.CustomInteractiveProjectShareCodeTextBox.Text);
                        ChannelSession.Settings.CustomInteractiveProjectIDs.Add(project);
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
            InteractiveSharedProjectModel project = (InteractiveSharedProjectModel)button.DataContext;
            ChannelSession.Settings.CustomInteractiveProjectIDs.Remove(project);
            this.customInteractiveProjects.Remove(project);
        }
    }
}
