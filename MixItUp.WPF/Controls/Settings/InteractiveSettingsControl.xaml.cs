using Mixer.Base.Model.Interactive;
using MixItUp.Base;
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
        private static readonly InteractiveGameListingModel NoneInteractiveGame = new InteractiveGameListingModel() { id = 0, name = "NONE" };

        private ObservableCollection<uint> customInteractiveProjects = new ObservableCollection<uint>();

        public InteractiveSettingsControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            List<InteractiveGameListingModel> interactiveGames = new List<InteractiveGameListingModel>();
            interactiveGames.Add(InteractiveSettingsControl.NoneInteractiveGame);
            interactiveGames.AddRange(await ChannelSession.Connection.GetOwnedInteractiveGames(ChannelSession.Channel));
            this.DefaultInteractiveGameComboBox.ItemsSource = interactiveGames;

            InteractiveGameListingModel game = interactiveGames.FirstOrDefault(g => g.id.Equals(ChannelSession.Settings.DefaultInteractiveGame));
            if (game == null) { game = InteractiveSettingsControl.NoneInteractiveGame; }
            this.DefaultInteractiveGameComboBox.SelectedItem = game;

            this.PreventUnknownInteractiveUsersToggleButton.IsChecked = ChannelSession.Settings.PreventUnknownInteractiveUsers;

            this.CustomInteractiveProjectsListView.ItemsSource = this.customInteractiveProjects;
            this.customInteractiveProjects.Clear();
            foreach (uint projectID in ChannelSession.Settings.CustomInteractiveProjectIDs)
            {
                this.customInteractiveProjects.Add(projectID);
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
                InteractiveGameListingModel game = (InteractiveGameListingModel)this.DefaultInteractiveGameComboBox.SelectedItem;
                ChannelSession.Settings.DefaultInteractiveGame = game.id;
            }
        }

        private void PreventUnknownInteractiveUsersToggleButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            ChannelSession.Settings.PreventUnknownInteractiveUsers = this.PreventUnknownInteractiveUsersToggleButton.IsChecked.GetValueOrDefault();
        }

        private void AddCustomInteractiveProjectButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.AddCustomInteractiveProjectTextBox.Text))
            {
                if (uint.TryParse(this.AddCustomInteractiveProjectTextBox.Text, out uint projectID) && !ChannelSession.Settings.CustomInteractiveProjectIDs.Contains(projectID))
                {
                    ChannelSession.Settings.CustomInteractiveProjectIDs.Add(projectID);
                    this.customInteractiveProjects.Add(projectID);
                }
            }
            this.AddCustomInteractiveProjectTextBox.Text = string.Empty;
        }

        private void DeleteCustomInteractiveProjectButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button button = (Button)sender;
            uint projectID = (uint)button.DataContext;
            ChannelSession.Settings.CustomInteractiveProjectIDs.Remove(projectID);
            this.customInteractiveProjects.Remove(projectID);
        }
    }
}
