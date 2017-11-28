using Mixer.Base.Model.Game;
using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.WPF.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Channel
{
    public enum AgeRatingEnum
    {
        Family,
        Teen,
        [Name("18+")]
        Adult,
    }

    /// <summary>
    /// Interaction logic for ChannelControl.xaml
    /// </summary>
    public partial class ChannelControl : MainControlBase
    {
        private ObservableCollection<GameTypeModel> relatedGames = new ObservableCollection<GameTypeModel>();


        public ChannelControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.GameNameComboBox.ItemsSource = this.relatedGames;
            this.AgeRatingComboBox.ItemsSource = EnumHelper.GetEnumNames<AgeRatingEnum>();

            this.StreamTitleTextBox.Text = ChannelSession.Channel.name;
            if (ChannelSession.Channel.type != null)
            {
                this.GameNameComboBox.Text = ChannelSession.Channel.type.name;
            }

            List<string> ageRatingList = EnumHelper.GetEnumNames<AgeRatingEnum>().Select(s => s.ToLower()).ToList();
            this.AgeRatingComboBox.SelectedIndex = ageRatingList.IndexOf(ChannelSession.Channel.audience);

            return base.InitializeInternal();
        }

        private async void GameNameComboBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(this.GameNameComboBox.Text) && this.GameNameComboBox.SelectedIndex < 0)
            {
                this.relatedGames.Clear();
                await this.GetRelatedGamesByName(this.GameNameComboBox.Text);
            }
        }

        private void GameNameTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(this.GameNameComboBox.Text) || this.GameNameComboBox.SelectedIndex < 0)
            {
                this.GameNameComboBox.SelectedItem = ChannelSession.Channel.type;
            }
        }

        private async void UpdateChannelDataButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.StreamTitleTextBox.Text))
            {
                await MessageBoxHelper.ShowMessageDialog("A stream title must be specified");
                return;
            }

            if (string.IsNullOrWhiteSpace(this.GameNameComboBox.Text) || this.GameNameComboBox.SelectedIndex < 0)
            {
                await MessageBoxHelper.ShowMessageDialog("A valid & existing game name must be selected");
                return;
            }

            if (this.AgeRatingComboBox.SelectedIndex < 0)
            {
                await MessageBoxHelper.ShowMessageDialog("A valid age rating must be selected");
                return;
            }

            await this.Window.RunAsyncOperation(async () =>
            {
                ChannelSession.Channel.name = this.StreamTitleTextBox.Text;
                ChannelSession.Channel.type = (GameTypeModel)this.GameNameComboBox.SelectedItem;
                ChannelSession.Channel.typeId = ChannelSession.Channel.type.id;
                ChannelSession.Channel.audience = ((string)this.AgeRatingComboBox.SelectedItem).ToLower();

                await ChannelSession.Connection.UpdateChannel(ChannelSession.Channel);
            });
        }

        private async Task GetRelatedGamesByName(string gameName)
        {
            if (!string.IsNullOrEmpty(gameName))
            {
                var games = await ChannelSession.Connection.GetGameTypes(gameName, 10);
                this.relatedGames.Clear();
                foreach (var game in games)
                {
                    this.relatedGames.Add(game);
                }
            }
        }
    }
}
