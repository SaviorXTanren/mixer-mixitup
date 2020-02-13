using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Game;
using Mixer.Base.Model.Teams;
using Mixer.Base.Model.User;
using MixItUp.Base;
using MixItUp.Base.Model.Favorites;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Favorites;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Favorites;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MixItUp.WPF.Controls.MainControls
{
    public enum RaidSearchCriteriaEnum
    {
        SameGame,
        SameTeam,
        SameAgeRating,
        SmallStreamer,
        MediumStreamer,
        LargeStreamer,
        PartneredStreamer,
        RandomStreamer,
    }

    public partial class ChannelControl : MainControlBase
    {
        private ObservableCollection<FavoriteGroupViewModel> favoritedGroups = new ObservableCollection<FavoriteGroupViewModel>();

        private ChannelModel channelToRaid;
        private bool shouldShowIntellisense = false;

        public ChannelControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.AgeRatingComboBox.ItemsSource = EnumHelper.GetEnumList<AgeRatingEnum>();

            this.StreamTitleComboBox.Text = ChannelSession.MixerChannel.name;
            List<string> streamTitles = new List<string>(ChannelSession.Settings.RecentStreamTitles);
            this.StreamTitleComboBox.ItemsSource = streamTitles;

            this.shouldShowIntellisense = false;
            if (ChannelSession.MixerChannel?.type?.name != null)
            {
                this.GameNameTextBox.Text = ChannelSession.MixerChannel.type.name;
            }
            else
            {
                this.GameNameTextBox.Text = MixItUp.Base.Resources.WebShow;
            }
            this.shouldShowIntellisense = true;

            AgeRatingEnum currentAgeRating = EnumHelper.GetEnumValueFromString<AgeRatingEnum>(ChannelSession.MixerChannel.audience);
            this.AgeRatingComboBox.SelectedItem = currentAgeRating;

            this.ChannelToRaidSearchCriteriaComboBox.ItemsSource = EnumHelper.GetEnumList<RaidSearchCriteriaEnum>();

            this.AddFavoriteTypeComboBox.ItemsSource = new List<string>() { MixItUp.Base.Resources.Team, MixItUp.Base.Resources.Streamer };
            this.AddFavoriteUserGroupNameTextBox.Text = MixItUp.Base.Resources.FavoriteStreamers;

            this.FavoriteGroupsDataGrid.ItemsSource = this.favoritedGroups;

            return base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.InitializeInternal();

            await this.RefreshFavoriteGroups();
        }

        private async void GameNameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            HideIntellisense();
            if (shouldShowIntellisense)
            {
                if (!string.IsNullOrEmpty(this.GameNameTextBox.Text))
                {
                    var games = (await ChannelSession.MixerUserConnection.GetGameTypes(this.GameNameTextBox.Text, 5))
                        .Take(5)
                        .ToList();
                    if (games.Count > 0)
                    {
                        GameNameIntellisenseListBox.ItemsSource = games;

                        // Select the first game
                        GameNameIntellisenseListBox.SelectedIndex = 0;

                        Rect positionOfCarat = this.GameNameTextBox.GetRectFromCharacterIndex(this.GameNameTextBox.CaretIndex, true);
                        if (!positionOfCarat.IsEmpty)
                        {
                            Point topLeftOffset = this.GameNameTextBox.TransformToAncestor(MainGrid).Transform(new Point(positionOfCarat.Left, positionOfCarat.Top));
                            ShowIntellisense(topLeftOffset.X, topLeftOffset.Y);
                        }
                    }
                }
            }
        }

        private void ShowIntellisense(double x, double y)
        {
            this.GameNameIntellisenseContent.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(this.GameNameIntellisense, x + 10);
            Canvas.SetTop(this.GameNameIntellisense, y - 10);
            this.GameNameIntellisense.UpdateLayout();

            if (!this.GameNameIntellisense.IsPopupOpen)
            {
                this.GameNameIntellisense.IsPopupOpen = true;
            }
        }

        private void GameNameTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (this.GameNameIntellisense.IsPopupOpen)
            {
                switch (e.Key)
                {
                    case Key.Escape:
                        HideIntellisense();
                        e.Handled = true;
                        break;
                    case Key.Tab:
                    case Key.Enter:
                        SelectIntellisenseGame();
                        e.Handled = true;
                        break;
                    case Key.Up:
                        GameNameIntellisenseListBox.SelectedIndex = MathHelper.Clamp(GameNameIntellisenseListBox.SelectedIndex - 1, 0, GameNameIntellisenseListBox.Items.Count - 1);
                        e.Handled = true;
                        break;
                    case Key.Down:
                        GameNameIntellisenseListBox.SelectedIndex = MathHelper.Clamp(GameNameIntellisenseListBox.SelectedIndex + 1, 0, GameNameIntellisenseListBox.Items.Count - 1);
                        e.Handled = true;
                        break;
                }
            }
        }

        private void GameNameIntellisenseListBox_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            SelectIntellisenseGame();
        }

        private async Task<GameTypeModel> GetValidGameType(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                return (await ChannelSession.MixerUserConnection.GetGameTypes(name, 15)).FirstOrDefault(g => g.name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            }

            return null;
        }

        private void SelectIntellisenseGame()
        {
            this.shouldShowIntellisense = false;
            GameTypeModel gameType = GameNameIntellisenseListBox.SelectedItem as GameTypeModel;
            if (gameType != null)
            {
                this.GameNameTextBox.Text = gameType.name;
                this.GameNameTextBox.CaretIndex = this.GameNameTextBox.Text.Length;
            }
            this.shouldShowIntellisense = true;

            HideIntellisense();
        }

        private void HideIntellisense()
        {
            if (this.GameNameIntellisense.IsPopupOpen)
            {
                this.GameNameIntellisense.IsPopupOpen = false;
            }
        }

        private async void UpdateChannelDataButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.StreamTitleComboBox.Text))
            {
                await DialogHelper.ShowMessage(MixItUp.Base.Resources.ChannelErrorNoStreamTitle);
                return;
            }

            GameTypeModel gameType = await GetValidGameType(this.GameNameTextBox.Text);
            if (gameType == null)
            {
                await DialogHelper.ShowMessage(MixItUp.Base.Resources.ChannelErrorInvalidGame);
                return;
            }

            if (this.AgeRatingComboBox.SelectedIndex < 0)
            {
                await DialogHelper.ShowMessage(MixItUp.Base.Resources.ChannelErrorInvalidAgeRating);
                return;
            }

            await this.Window.RunAsyncOperation(async () =>
            {
                string age = EnumHelper.GetEnumName<AgeRatingEnum>((AgeRatingEnum)this.AgeRatingComboBox.SelectedItem).ToLower();
                await ChannelSession.MixerUserConnection.UpdateChannel(ChannelSession.MixerChannel.id, this.StreamTitleComboBox.Text, gameType.id, age);

                await ChannelSession.RefreshChannel();

                List<string> streamTitles = new List<string>(ChannelSession.Settings.RecentStreamTitles);
                this.StreamTitleComboBox.ItemsSource = streamTitles;
            });
        }

        private async void FindChannelToRaidButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                if (this.ChannelToRaidSearchCriteriaComboBox.SelectedIndex < 0)
                {
                    await DialogHelper.ShowMessage(MixItUp.Base.Resources.FindRaidErrorNoCriteria);
                    return;
                }

                await ChannelSession.RefreshChannel();

                IEnumerable<ChannelModel> channels = null;
                RaidSearchCriteriaEnum searchCriteria = (RaidSearchCriteriaEnum)this.ChannelToRaidSearchCriteriaComboBox.SelectedItem;
                if (searchCriteria == RaidSearchCriteriaEnum.SameGame)
                {
                    channels = await ChannelSession.MixerUserConnection.GetChannelsByGameTypes(ChannelSession.MixerChannel.type, 1);
                }
                else if (searchCriteria == RaidSearchCriteriaEnum.SameTeam)
                {
                    Dictionary<uint, UserWithChannelModel> teamChannels = new Dictionary<uint, UserWithChannelModel>();
                    foreach (TeamMembershipExpandedModel extendedTeam in await ChannelSession.MixerUserConnection.GetUserTeams(ChannelSession.MixerUser))
                    {
                        TeamModel team = await ChannelSession.MixerUserConnection.GetTeam(extendedTeam.id);
                        IEnumerable<UserWithChannelModel> teamUsers = await ChannelSession.MixerUserConnection.GetTeamUsers(team);
                        foreach (UserWithChannelModel userChannel in teamUsers.Where(u => u.channel.online))
                        {
                            teamChannels[userChannel.id] = userChannel;
                        }
                    }
                    channels = teamChannels.Values.Select(c => c.channel);
                }
                else
                {
                    string filters = string.Empty;
                    if (searchCriteria == RaidSearchCriteriaEnum.SameAgeRating)
                    {
                        filters = "audience:eq:" + ChannelSession.MixerChannel.audience;
                    }
                    else if (searchCriteria == RaidSearchCriteriaEnum.LargeStreamer)
                    {
                        filters = "viewersCurrent:gte:25";
                    }
                    else if (searchCriteria == RaidSearchCriteriaEnum.MediumStreamer)
                    {
                        filters = "viewersCurrent:gte:10,viewersCurrent:lt:25";
                    }
                    else if (searchCriteria == RaidSearchCriteriaEnum.SmallStreamer)
                    {
                        filters = "viewersCurrent:gt:0,viewersCurrent:lt:10";
                    }
                    else if (searchCriteria == RaidSearchCriteriaEnum.PartneredStreamer)
                    {
                        filters = "partnered:eq:true";
                    }
                    channels = await ChannelSession.MixerUserConnection.Connection.Channels.GetChannels(filters, 50);
                }

                this.ChannelRaidNameTextBox.Clear();
                if (channels != null && channels.Count() > 0)
                {
                    this.channelToRaid = channels.ElementAt(RandomHelper.GenerateRandomNumber(channels.Count()));

                    UserModel user = await ChannelSession.MixerUserConnection.GetUser(this.channelToRaid.userId);
                    GameTypeModel game = await ChannelSession.MixerUserConnection.GetGameType(this.channelToRaid.typeId.GetValueOrDefault());

                    this.ChannelRaidNameTextBox.Text = user.username;
                    this.ChannelRaidViewersTextBox.Text = this.channelToRaid.viewersCurrent.ToString();
                    this.ChannelRaidAudienceTextBox.Text = EnumHelper.GetEnumName(EnumHelper.GetEnumValueFromString<AgeRatingEnum>(this.channelToRaid.audience));
                    this.ChannelRaidGameTextBox.Text = (game != null) ? game.name : MixItUp.Base.Resources.Unknown;
                }
                else
                {
                    await DialogHelper.ShowMessage(MixItUp.Base.Resources.FindRaidErrorNoneFound);
                }
            });
        }

        private void AddFavoriteTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            this.AddFavoriteTeamGrid.Visibility = System.Windows.Visibility.Collapsed;
            this.AddFavoriteUserGrid.Visibility = System.Windows.Visibility.Collapsed;
            if (this.AddFavoriteTypeComboBox.SelectedIndex >= 0)
            {
                string selection = (string)this.AddFavoriteTypeComboBox.SelectedItem;
                if (selection.Equals(MixItUp.Base.Resources.Team))
                {
                    this.AddFavoriteTeamGrid.Visibility = System.Windows.Visibility.Visible;
                }
                else if (selection.Equals(MixItUp.Base.Resources.Streamer))
                {
                    this.AddFavoriteUserGrid.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        private async void AddFavoriteButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                if (this.AddFavoriteTypeComboBox.SelectedIndex >= 0)
                {
                    string selection = (string)this.AddFavoriteTypeComboBox.SelectedItem;
                    if (selection.Equals(MixItUp.Base.Resources.Team))
                    {
                        if (!string.IsNullOrEmpty(this.AddFavoriteTeamTextBox.Text))
                        {
                            TeamModel team = await ChannelSession.MixerUserConnection.GetTeam(this.AddFavoriteTeamTextBox.Text);
                            if (team != null)
                            {
                                if (ChannelSession.Settings.FavoriteGroups.Any(t => t.Team != null && t.Team.id.Equals(team.id)))
                                {
                                    await DialogHelper.ShowMessage(MixItUp.Base.Resources.AddFavoriteTeamAlreadyDone);
                                    return;
                                }

                                ChannelSession.Settings.FavoriteGroups.Add(new FavoriteGroupModel(team));
                                await ChannelSession.SaveSettings();

                                await this.RefreshFavoriteGroups();
                            }
                            else
                            {
                                await DialogHelper.ShowMessage(MixItUp.Base.Resources.AddFavoriteTeamNotFound);
                            }
                        }
                    }
                    else if (selection.Equals(MixItUp.Base.Resources.Streamer))
                    {
                        if (!string.IsNullOrEmpty(this.AddFavoriteUserTextBox.Text) && !string.IsNullOrEmpty(this.AddFavoriteUserGroupNameTextBox.Text))
                        {
                            UserModel user = await ChannelSession.MixerUserConnection.GetUser(this.AddFavoriteUserTextBox.Text);
                            if (user != null)
                            {
                                FavoriteGroupModel group = ChannelSession.Settings.FavoriteGroups.FirstOrDefault(t => t.GroupName != null && t.GroupName.Equals(this.AddFavoriteUserGroupNameTextBox.Text));
                                if (group != null)
                                {
                                    if (group.GroupUserIDs.Any(id => id.Equals(user.id)))
                                    {
                                        await DialogHelper.ShowMessage(MixItUp.Base.Resources.AddFavoriteStreamerAlreadyDone);
                                        return;
                                    }
                                }

                                if (group == null)
                                {
                                    group = new FavoriteGroupModel(this.AddFavoriteUserGroupNameTextBox.Text);
                                    ChannelSession.Settings.FavoriteGroups.Add(group);
                                }
                                group.GroupUserIDs.Add(user.id);
                                await ChannelSession.SaveSettings();

                                await this.RefreshFavoriteGroups();
                            }
                            else
                            {
                                await DialogHelper.ShowMessage(MixItUp.Base.Resources.AddFavoriteStreamerNotFound);
                            }
                        }
                    }
                }
            });
        }

        private void ViewFavoriteGroupButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button button = (Button)sender;
            FavoriteGroupViewModel group = (FavoriteGroupViewModel)button.DataContext;
            FavoriteGroupWindow window = new FavoriteGroupWindow(group);
            window.Closed += Window_Closed;
            window.Show();
        }

        private async void DeleteFavoriteGroupButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button button = (Button)sender;
            FavoriteGroupViewModel group = (FavoriteGroupViewModel)button.DataContext;
            await this.Window.RunAsyncOperation(async () =>
            {
                if (await DialogHelper.ShowConfirmation(MixItUp.Base.Resources.DeleteFavoriteGroupConfirmation))
                {
                    ChannelSession.Settings.FavoriteGroups.Remove(group.Group);
                    await ChannelSession.SaveSettings();

                    await this.RefreshFavoriteGroups();
                }
            });
        }

        private async void Window_Closed(object sender, EventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                await this.RefreshFavoriteGroups();
            });
        }

        private async Task RefreshFavoriteGroups()
        {
            this.AddFavoriteTeamTextBox.Clear();
            this.AddFavoriteUserTextBox.Clear();

            this.favoritedGroups.Clear();
            foreach (FavoriteGroupModel group in ChannelSession.Settings.FavoriteGroups)
            {
                FavoriteGroupViewModel groupViewModel = new FavoriteGroupViewModel(group);
                await groupViewModel.RefreshGroup();
                this.favoritedGroups.Add(groupViewModel);
            }
        }

        private async void HostChannelButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.channelToRaid != null)
            {
                await this.Window.RunAsyncOperation(async () =>
                {
                    await ChannelSession.MixerUserConnection.SetHostChannel(ChannelSession.MixerChannel, this.channelToRaid);
                });
            }
        }
    }
}
