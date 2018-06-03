using Mixer.Base.Model.Interactive;
using MixItUp.Base;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Settings
{
    /// <summary>
    /// Interaction logic for InteractiveSettingsControl.xaml
    /// </summary>
    public partial class InteractiveSettingsControl : SettingsControlBase
    {
        private static readonly InteractiveGameListingModel NoneInteractiveGame = new InteractiveGameListingModel() { id = 0, name = "NONE" };

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
    }
}
