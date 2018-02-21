using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Game;
using Mixer.Base.Model.Teams;
using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.WPF.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.MainControls
{
    public enum AgeRatingEnum
    {
        Family,
        Teen,
        [Name("18+")]
        Adult,
    }

    public enum RaidSearchCriteriaEnum
    {
        [Name("Same Game")]
        SameGame,
        [Name("Same Team")]
        SameTeam,
        [Name("Same Age Rating")]
        AgeRating,
        [Name("Small Streamer (> 10)")]
        SmallStreamer,
        [Name("Medium Streamer (10-25)")]
        MediumStreamer,
        [Name("Large Streamer (< 25)")]
        LargeStreamer,
        [Name("Partnered Streamer")]
        PartneredStreamer,
        [Name("Random Streamer")]
        Random,
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

            this.ChannelToRaidSearchCriteriaComboBox.ItemsSource = EnumHelper.GetEnumNames<RaidSearchCriteriaEnum>();

            return base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.InitializeInternal();
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

        private async void FindChannelToRaidButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                if (this.ChannelToRaidSearchCriteriaComboBox.SelectedIndex < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("Must select a search criteria for finding a channel to raid");
                    return;
                }

                await ChannelSession.RefreshChannel();

                IEnumerable<ChannelModel> channels = null;
                RaidSearchCriteriaEnum searchCriteria = EnumHelper.GetEnumValueFromString<RaidSearchCriteriaEnum>((string)this.ChannelToRaidSearchCriteriaComboBox.SelectedItem);
                if (searchCriteria == RaidSearchCriteriaEnum.SameGame)
                {
                    channels = await ChannelSession.Connection.GetChannelsByGameTypes(ChannelSession.Channel.type, 1);
                }
                else if (searchCriteria == RaidSearchCriteriaEnum.SameTeam)
                {
                    Dictionary<uint, UserWithChannelModel> teamChannels = new Dictionary<uint, UserWithChannelModel>();
                    foreach (TeamMembershipExpandedModel extendedTeam in await ChannelSession.Connection.GetUserTeams(ChannelSession.User))
                    {
                        TeamModel team = await ChannelSession.Connection.GetTeam(extendedTeam.id);
                        IEnumerable<UserWithChannelModel> teamUsers = await ChannelSession.Connection.GetTeamUsers(team);
                        foreach (UserWithChannelModel userChannel in teamUsers.Where(u => u.channel.online))
                        {
                            teamChannels[userChannel.id] = userChannel;
                        }
                    }
                    channels = teamChannels.Values.Select(c => c.channel);
                }
                else
                {
                    string query = "channels";
                    if (searchCriteria == RaidSearchCriteriaEnum.AgeRating)
                    {
                        query += "?where=audience:eq:" + ChannelSession.Channel.audience;
                    }
                    else if (searchCriteria == RaidSearchCriteriaEnum.LargeStreamer)
                    {
                        query += "?where=viewersCurrent:gte:25";
                    }
                    else if (searchCriteria == RaidSearchCriteriaEnum.MediumStreamer)
                    {
                        query += "?where=viewersCurrent:gte:10,viewersCurrent:lt:25";
                    }
                    else if (searchCriteria == RaidSearchCriteriaEnum.SmallStreamer)
                    {
                        query += "?where=viewersCurrent:gt:0,viewersCurrent:lt:10";
                    }
                    else if (searchCriteria == RaidSearchCriteriaEnum.PartneredStreamer)
                    {
                        query += "?where=partnered:eq:true";
                    }
                    channels = await ChannelSession.Connection.Connection.Channels.GetPagedAsync<ChannelModel>(query, 50, linkPagesAvailable: false);
                }

                this.ChannelRaidNameTextBox.Clear();
                if (channels != null && channels.Count() > 0)
                {
                    Random random = new Random();
                    ChannelModel channelToRaid = channels.ElementAt(random.Next(0, channels.Count()));

                    UserModel user = await ChannelSession.Connection.GetUser(channelToRaid.userId);
                    GameTypeModel game = await ChannelSession.Connection.GetGameType(channelToRaid.typeId.GetValueOrDefault());

                    this.ChannelRaidNameTextBox.Text = user.username;
                    this.ChannelRaidViewersTextBox.Text = channelToRaid.viewersCurrent.ToString();
                    this.ChannelRaidAudienceTextBox.Text = EnumHelper.GetEnumName(EnumHelper.GetEnumValueFromString<AgeRatingEnum>(channelToRaid.audience));
                    this.ChannelRaidGameTextBox.Text = (game != null) ? game.name : "Unknown";
                }
                else
                {
                    await MessageBoxHelper.ShowMessageDialog("Unable to find a channel that met your search critera, please try selecting a different option");
                }
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
