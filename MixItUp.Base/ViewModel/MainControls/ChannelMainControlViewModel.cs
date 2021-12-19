using MixItUp.Base.Model;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Twitch.Base.Models.NewAPI.Channels;
using Twitch.Base.Models.NewAPI.Games;
using Twitch.Base.Models.NewAPI.Streams;
using Twitch.Base.Models.NewAPI.Tags;
using Twitch.Base.Models.NewAPI.Teams;
using Twitch.Base.Models.NewAPI.Users;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class TagViewModel : UIViewModelBase
    {
        public TagModel Tag
        {
            get { return this.tag; }
            set
            {
                this.tag = value;
                this.NotifyPropertyChanged();
            }
        }
        private TagModel tag;

        public ICommand DeleteTagCommand { get; private set; }

        private ChannelMainControlViewModel viewModel;

        public TagViewModel(ChannelMainControlViewModel viewModel, TagModel tag)
        {
            this.viewModel = viewModel;
            this.Tag = tag;

            this.DeleteTagCommand = this.CreateCommand(() =>
            {
                this.viewModel.RemoveTag(this);
            });
        }

        public string ID { get { return this.Tag.tag_id; } }

        public string Name
        {
            get
            {
                if (this.tag.localization_names.ContainsKey("en-us"))
                {
                    return (string)this.tag.localization_names["en-us"];
                }
                return "Tag";
            }
        }

        public bool IsDeletable { get { return !this.Tag.is_auto; } }
    }

    public enum SearchFindChannelToRaidTypeEnum
    {
        FollowedChannels,
        TeamMembers,
        SameGame,
        SameLanguage,
        Featured,
    }

    public class SearchFindChannelToRaidItemViewModel : UIViewModelBase
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public long Viewers { get; set; }
        public string GameName { get; set; }

        public ICommand OpenChannelCommand { get; private set; }
        public ICommand RaidChannelCommand { get; private set; }

        public SearchFindChannelToRaidItemViewModel(Twitch.Base.Models.NewAPI.Streams.StreamModel stream)
            : this()
        {
            this.ID = stream.user_id;
            this.Name = stream.user_login;
            this.Viewers = stream.viewer_count;
            this.GameName = stream.game_name;
        }

        public SearchFindChannelToRaidItemViewModel(Twitch.Base.Models.NewAPI.Streams.StreamModel stream, GameModel game)
            : this()
        {
            this.ID = stream.user_id;
            this.Name = stream.user_name;
            this.Viewers = stream.viewer_count;
            this.GameName = (game != null) ? game.name : "Unknown";
        }

        private SearchFindChannelToRaidItemViewModel()
        {
            this.OpenChannelCommand = this.CreateCommand(() =>
            {
                ProcessHelper.LaunchLink(this.URL);
            });

            this.RaidChannelCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<ChatService>().SendMessage("/raid @" + this.Name, sendAsStreamer: true, platform: StreamingPlatformTypeEnum.Twitch);
            });
        }

        public string URL { get { return $"https://www.twitch.tv/{this.Name}"; } }
    }

    public class ChannelMainControlViewModel : WindowControlViewModelBase
    {
        public ThreadSafeObservableCollection<string> PastTitles { get; private set; } = new ThreadSafeObservableCollection<string>();

        public string Title
        {
            get { return this.title; }
            set
            {
                this.title = value;
                this.NotifyPropertyChanged();
            }
        }
        private string title;

        public ThreadSafeObservableCollection<string> PastGameNames { get; private set; } = new ThreadSafeObservableCollection<string>();

        public string GameName
        {
            get { return this.gameName; }
            set
            {
                this.gameName = value;
                this.NotifyPropertyChanged();
            }
        }
        private string gameName;

        public ThreadSafeObservableCollection<TagViewModel> Tags { get; private set; } = new ThreadSafeObservableCollection<TagViewModel>();

        public TagViewModel Tag
        {
            get { return this.tag; }
            set
            {
                this.tag = value;
                this.NotifyPropertyChanged();
            }
        }
        private TagViewModel tag;

        public ThreadSafeObservableCollection<TagViewModel> CustomTags { get; private set; } = new ThreadSafeObservableCollection<TagViewModel>();

        public ChannelInformationModel ChannelInformation { get; private set; }

        public ICommand AddTagCommand { get; private set; }

        public bool CanAddMoreTags { get { return this.CustomTags.Count < 5; } }

        public ICommand UpdateChannelInformationCommand { get; private set; }

        public List<SearchFindChannelToRaidTypeEnum> SearchFindChannelToRaidOptions { get; private set; } = new List<SearchFindChannelToRaidTypeEnum>(EnumHelper.GetEnumList<SearchFindChannelToRaidTypeEnum>());

        public SearchFindChannelToRaidTypeEnum SelectedSearchFindChannelToRaidOption
        {
            get { return this.selectedSearchFindChannelToRaidOption; }
            set
            {
                this.selectedSearchFindChannelToRaidOption = value;
                this.NotifyPropertyChanged();
            }
        }
        private SearchFindChannelToRaidTypeEnum selectedSearchFindChannelToRaidOption;

        public ICommand SearchFindChannelToRaidCommand { get; private set; }

        public ThreadSafeObservableCollection<SearchFindChannelToRaidItemViewModel> SearchFindChannelToRaidResults { get; private set; } = new ThreadSafeObservableCollection<SearchFindChannelToRaidItemViewModel>();

        private GameModel currentGame;

        public ChannelMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            this.AddTagCommand = this.CreateCommand(() =>
            {
                if (this.Tag != null && !this.CustomTags.Contains(tag))
                {
                    this.CustomTags.Add(tag);
                    this.Tag = null;
                }
                this.NotifyPropertyChanged("CanAddMoreTags");
            });

            this.UpdateChannelInformationCommand = this.CreateCommand(async () =>
            {
                // TODO

                bool failedToFindGame = false;
                if (this.currentGame != null && !string.IsNullOrEmpty(this.GameName) && this.GameName.Length > 3 && !string.Equals(this.currentGame.name, this.GameName, StringComparison.InvariantCultureIgnoreCase))
                {
                    IEnumerable<GameModel> games = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetNewAPIGamesByName(this.GameName);
                    if (games != null && games.Count() > 0)
                    {
                        this.currentGame = games.First();
                    }
                    else
                    {
                        failedToFindGame = true;
                    }
                }

                await ServiceManager.Get<TwitchSessionService>().UserConnection.UpdateChannelInformation(ServiceManager.Get<TwitchSessionService>().User, this.Title, this.currentGame?.id);

                IEnumerable<TagModel> tags = this.CustomTags.Select(t => t.Tag);
                await ServiceManager.Get<TwitchSessionService>().UserConnection.UpdateStreamTagsForChannel(ServiceManager.Get<TwitchSessionService>().User, tags);

                await this.RefreshChannelInformation();

                if (failedToFindGame)
                {
                    await DialogHelper.ShowMessage(MixItUp.Base.Resources.FailedToUpdateGame);
                }
            });

            this.SearchFindChannelToRaidCommand = this.CreateCommand(async () =>
            {
                // TODO

                this.SearchFindChannelToRaidResults.Clear();

                List<SearchFindChannelToRaidItemViewModel> results = new List<SearchFindChannelToRaidItemViewModel>();

                if (this.SelectedSearchFindChannelToRaidOption == SearchFindChannelToRaidTypeEnum.Featured)
                {
                    foreach (StreamModel stream in await ServiceManager.Get<TwitchSessionService>().UserConnection.GetTopStreams(10))
                    {
                        results.Add(new SearchFindChannelToRaidItemViewModel(stream));
                    }
                }
                else if (this.SelectedSearchFindChannelToRaidOption == SearchFindChannelToRaidTypeEnum.SameGame && this.currentGame != null)
                {
                    IEnumerable<StreamModel> streams = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetGameStreams(this.currentGame.id, 10);
                    if (streams.Count() > 0)
                    {
                        Dictionary<string, GameModel> games = new Dictionary<string, GameModel>();
                        foreach (GameModel game in await ServiceManager.Get<TwitchSessionService>().UserConnection.GetNewAPIGamesByIDs(streams.Select(s => s.game_id)))
                        {
                            games[game.id] = game;
                        }

                        foreach (StreamModel stream in streams)
                        {
                            results.Add(new SearchFindChannelToRaidItemViewModel(stream, games.ContainsKey(stream.game_id) ? games[stream.game_id] : null));
                        }
                    }
                }
                else if (this.SelectedSearchFindChannelToRaidOption == SearchFindChannelToRaidTypeEnum.SameLanguage && this.ChannelInformation != null)
                {
                    IEnumerable<StreamModel> streams = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetLanguageStreams(this.ChannelInformation.broadcaster_language, 10);
                    if (streams.Count() > 0)
                    {
                        Dictionary<string, GameModel> games = new Dictionary<string, GameModel>();
                        foreach (GameModel game in await ServiceManager.Get<TwitchSessionService>().UserConnection.GetNewAPIGamesByIDs(streams.Select(s => s.game_id)))
                        {
                            games[game.id] = game;
                        }

                        foreach (StreamModel stream in streams)
                        {
                            results.Add(new SearchFindChannelToRaidItemViewModel(stream, games.ContainsKey(stream.game_id) ? games[stream.game_id] : null));
                        }
                    }
                }
                else if (this.SelectedSearchFindChannelToRaidOption == SearchFindChannelToRaidTypeEnum.FollowedChannels)
                {
                    foreach (StreamModel stream in await ServiceManager.Get<TwitchSessionService>().UserConnection.GetFollowedStreams(ServiceManager.Get<TwitchSessionService>().User, 10))
                    {
                        results.Add(new SearchFindChannelToRaidItemViewModel(stream));
                    }
                }
                else if (this.SelectedSearchFindChannelToRaidOption == SearchFindChannelToRaidTypeEnum.TeamMembers)
                {
                    List<UserModel> users = new List<UserModel>();
                    foreach (TeamModel team in await ServiceManager.Get<TwitchSessionService>().UserConnection.GetChannelTeams(ServiceManager.Get<TwitchSessionService>().User))
                    {
                        TeamDetailsModel teamDetails = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetTeam(team.id);
                        if (teamDetails != null && teamDetails.users != null)
                        {
                            foreach (TeamMemberModel user in teamDetails.users)
                            {
                                users.Add(new UserModel()
                                {
                                    id = user.user_id,
                                    login = user.user_login,
                                    display_name = user.user_name
                                });
                            }
                        }
                    }

                    if (users.Count > 0)
                    {
                        foreach (StreamModel stream in await ServiceManager.Get<TwitchSessionService>().UserConnection.GetStreams(users.Select(u => u.id)))
                        {
                            results.Add(new SearchFindChannelToRaidItemViewModel(stream));
                        }
                    }
                }

                this.SearchFindChannelToRaidResults.AddRange(results.Take(10));
            });
        }

        public void RemoveTag(TagViewModel tag)
        {
            this.CustomTags.Remove(tag);
            this.NotifyPropertyChanged("CanAddMoreTags");
        }

        protected override async Task OnLoadedInternal()
        {
            this.PastTitles.AddRange(ChannelSession.Settings.RecentStreamTitles);

            this.PastGameNames.AddRange(ChannelSession.Settings.RecentStreamGames);

            if (ServiceManager.Get<TwitchSessionService>().IsConnected)
            {
                List<TagViewModel> tags = new List<TagViewModel>();
                foreach (TagModel tag in await ServiceManager.Get<TwitchSessionService>().UserConnection.GetStreamTags())
                {
                    if (!tag.is_auto)
                    {
                        tags.Add(new TagViewModel(this, tag));
                    }
                }

            this.Tags.ClearAndAddRange(tags.OrderBy(t => t.Name));

            }

            await base.OnLoadedInternal();
        }

        protected override async Task OnVisibleInternal()
        {
            await this.RefreshChannelInformation();

            await base.OnVisibleInternal();
        }

        private async Task RefreshChannelInformation()
        {
            // TODO

            if (ServiceManager.Get<TwitchSessionService>().IsConnected)
            {
                this.ChannelInformation = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetChannelInformation(ServiceManager.Get<TwitchSessionService>().User);
                if (this.ChannelInformation != null)
                {
                    if (!string.IsNullOrEmpty(this.ChannelInformation.title))
                    {
                        this.Title = this.ChannelInformation.title;
                        if (!ChannelSession.Settings.RecentStreamTitles.Contains(this.ChannelInformation.title))
                        {
                            ChannelSession.Settings.RecentStreamTitles.Insert(0, this.ChannelInformation.title);
                            while (ChannelSession.Settings.RecentStreamTitles.Count > 5)
                            {
                                ChannelSession.Settings.RecentStreamTitles.RemoveAt(ChannelSession.Settings.RecentStreamTitles.Count - 1);
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(this.ChannelInformation.game_id) && !string.IsNullOrEmpty(this.ChannelInformation.game_name))
                    {
                        this.currentGame = new GameModel()
                        {
                            id = this.ChannelInformation.game_id,
                            name = this.ChannelInformation.game_name
                        };

                        this.GameName = this.currentGame.name;

                        if (!ChannelSession.Settings.RecentStreamGames.Contains(this.currentGame.name))
                        {
                            ChannelSession.Settings.RecentStreamGames.Insert(0, this.currentGame.name);
                            while (ChannelSession.Settings.RecentStreamGames.Count > 5)
                            {
                                ChannelSession.Settings.RecentStreamGames.RemoveAt(ChannelSession.Settings.RecentStreamTitles.Count - 1);
                            }
                        }
                    }
                }
            }

            // TODO
            if (ServiceManager.Get<TwitchSessionService>().IsConnected)
            {
                List<TagViewModel> tags = new List<TagViewModel>();
                foreach (TagModel tag in await ServiceManager.Get<TwitchSessionService>().UserConnection.GetStreamTagsForChannel(ServiceManager.Get<TwitchSessionService>().User))
                {
                    if (!tag.is_auto)
                    {
                        TagViewModel tagViewModel = this.Tags.FirstOrDefault(t => string.Equals(t.ID, tag.tag_id));
                        if (tagViewModel != null)
                        {
                            tags.Add(tagViewModel);
                        }
                    }
                }
                this.CustomTags.ClearAndAddRange(tags);
            }

            this.NotifyPropertyChanged("CanAddMoreTags");
        }
    }
}
