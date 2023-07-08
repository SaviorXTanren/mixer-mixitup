using MixItUp.Base.Model;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Trovo;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Twitch;
using MixItUp.Base.ViewModels;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Trovo.Base.Models.Channels;
using Twitch.Base.Models.NewAPI.Channels;
using Twitch.Base.Models.NewAPI.Games;
using Twitch.Base.Models.NewAPI.Streams;
using Twitch.Base.Models.NewAPI.Teams;
using Twitch.Base.Models.NewAPI.Users;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class TwitchChannelControlViewModel : PlatformChannelControlViewModelBase
    {
        public enum TwitchSearchFindChannelToRaidTypeEnum
        {
            FollowedChannels,
            TeamMembers,
            SameCategory,
            SameLanguage,
            Featured,
        }

        public IEnumerable<TwitchSearchFindChannelToRaidTypeEnum> SearchFindChannelToRaidOptions { get; set; } = EnumHelper.GetEnumList<TwitchSearchFindChannelToRaidTypeEnum>();

        public TwitchSearchFindChannelToRaidTypeEnum SelectedSearchFindChannelToRaidOption
        {
            get { return this.selectedSearchFindChannelToRaidOption; }
            set
            {
                this.selectedSearchFindChannelToRaidOption = value;
                this.NotifyPropertyChanged();
            }
        }
        private TwitchSearchFindChannelToRaidTypeEnum selectedSearchFindChannelToRaidOption;

        public ChannelInformationModel ChannelInformation { get; private set; }

        public TwitchTagEditorViewModel TagEditor { get; set; } = new TwitchTagEditorViewModel();

        private GameModel currentGame;

        public TwitchChannelControlViewModel() { this.Platform = StreamingPlatformTypeEnum.Twitch; }

        protected override async Task OnOpenInternal()
        {
            await this.TagEditor.OnOpen();

            await this.TagEditor.LoadCurrentTags();

            await base.OnOpenInternal();
        }

        protected override async Task<Result> UpdateChannelInformation()
        {
            Result result = await base.UpdateChannelInformation();
            if (!result.Success)
            {
                return result;
            }

            if (!await ServiceManager.Get<TwitchSessionService>().UserConnection.UpdateChannelInformation(ServiceManager.Get<TwitchSessionService>().User, tags: this.TagEditor.CustomTags.Select(t => t.Tag)))
            {
                return new Result(MixItUp.Base.Resources.TwitchFailedToUpdateCustomTags);
            }

            return new Result();
        }

        protected override async Task RefreshChannelInformation()
        {
            this.ChannelInformation = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetChannelInformation(ServiceManager.Get<TwitchSessionService>().User);
            if (this.ChannelInformation != null)
            {
                if (!string.IsNullOrEmpty(this.ChannelInformation.title))
                {
                    this.Title = this.ChannelInformation.title;
                }

                if (!string.IsNullOrEmpty(this.ChannelInformation.game_id) && !string.IsNullOrEmpty(this.ChannelInformation.game_name))
                {
                    this.currentGame = new GameModel()
                    {
                        id = this.ChannelInformation.game_id,
                        name = this.ChannelInformation.game_name
                    };

                    this.Category = this.currentGame.name;
                }

                this.TagEditor.ClearCustomTags();
                if (this.ChannelInformation.tags != null)
                {
                    foreach (string tag in this.ChannelInformation.tags)
                    {
                        await this.TagEditor.AddCustomTag(tag);
                    }
                }
            }
        }

        protected override async Task SearchChannelsToRaid()
        {
            this.ChannelsToRaid.Clear();

            List<ChannelToRaidItemViewModel> results = new List<ChannelToRaidItemViewModel>();

            if (this.SelectedSearchFindChannelToRaidOption == TwitchSearchFindChannelToRaidTypeEnum.Featured)
            {
                foreach (StreamModel stream in await ServiceManager.Get<TwitchSessionService>().UserConnection.GetTopStreams(10))
                {
                    results.Add(new ChannelToRaidItemViewModel(stream));
                }
            }
            else if (this.SelectedSearchFindChannelToRaidOption == TwitchSearchFindChannelToRaidTypeEnum.SameCategory && this.currentGame != null)
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
                        results.Add(new ChannelToRaidItemViewModel(stream, games.ContainsKey(stream.game_id) ? games[stream.game_id] : null));
                    }
                }
            }
            else if (this.SelectedSearchFindChannelToRaidOption == TwitchSearchFindChannelToRaidTypeEnum.SameLanguage && this.ChannelInformation != null)
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
                        results.Add(new ChannelToRaidItemViewModel(stream, games.ContainsKey(stream.game_id) ? games[stream.game_id] : null));
                    }
                }
            }
            else if (this.SelectedSearchFindChannelToRaidOption == TwitchSearchFindChannelToRaidTypeEnum.FollowedChannels)
            {
                foreach (StreamModel stream in await ServiceManager.Get<TwitchSessionService>().UserConnection.GetFollowedStreams(ServiceManager.Get<TwitchSessionService>().User, 10))
                {
                    results.Add(new ChannelToRaidItemViewModel(stream));
                }
            }
            else if (this.SelectedSearchFindChannelToRaidOption == TwitchSearchFindChannelToRaidTypeEnum.TeamMembers)
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
                    foreach (var userBatch in users.Batch(20))
                    {
                        foreach (StreamModel stream in await ServiceManager.Get<TwitchSessionService>().UserConnection.GetStreams(userBatch.Select(u => u.id)))
                        {
                            results.Add(new ChannelToRaidItemViewModel(stream));
                        }
                    }
                }
            }

            this.ChannelsToRaid.AddRange(results.Take(10));
        }
    }

    public class TrovoChannelControlViewModel : PlatformChannelControlViewModelBase
    {
        public enum TrovoSearchFindChannelToRaidTypeEnum
        {
            SameCategory,
            Featured,
        }

        public IEnumerable<TrovoSearchFindChannelToRaidTypeEnum> SearchFindChannelToRaidOptions { get; set; } = EnumHelper.GetEnumList<TrovoSearchFindChannelToRaidTypeEnum>();

        public TrovoSearchFindChannelToRaidTypeEnum SelectedSearchFindChannelToRaidOption
        {
            get { return this.selectedSearchFindChannelToRaidOption; }
            set
            {
                this.selectedSearchFindChannelToRaidOption = value;
                this.NotifyPropertyChanged();
            }
        }
        private TrovoSearchFindChannelToRaidTypeEnum selectedSearchFindChannelToRaidOption;

        public TrovoChannelControlViewModel() { this.Platform = StreamingPlatformTypeEnum.Trovo; }

        protected override async Task OnOpenInternal()
        {
            await base.OnOpenInternal();
        }

        protected override async Task SearchChannelsToRaid()
        {
            this.ChannelsToRaid.Clear();

            List<ChannelToRaidItemViewModel> results = new List<ChannelToRaidItemViewModel>();

            if (this.SelectedSearchFindChannelToRaidOption == TrovoSearchFindChannelToRaidTypeEnum.Featured)
            {
                foreach (TopChannelModel channel in await ServiceManager.Get<TrovoSessionService>().UserConnection.GetTopChannels(maxResults: 10))
                {
                    results.Add(new ChannelToRaidItemViewModel(channel));
                }
            }
            else if (this.SelectedSearchFindChannelToRaidOption == TrovoSearchFindChannelToRaidTypeEnum.SameCategory)
            {
                foreach (TopChannelModel channel in await ServiceManager.Get<TrovoSessionService>().UserConnection.GetTopChannels(maxResults: 10, category: ServiceManager.Get<TrovoSessionService>().Channel.category_id))
                {
                    results.Add(new ChannelToRaidItemViewModel(channel));
                }
            }

            this.ChannelsToRaid.AddRange(results.Take(10));
        }
    }

    public abstract class PlatformChannelControlViewModelBase : UIViewModelBase
    {
        public class ChannelToRaidItemViewModel : UIViewModelBase
        {
            public StreamingPlatformTypeEnum Platform { get; set; }
            public string ID { get; set; }
            public string Name { get; set; }
            public long Viewers { get; set; }
            public string Category { get; set; }

            public ICommand OpenChannelCommand { get; private set; }
            public ICommand RaidChannelCommand { get; private set; }

            public ChannelToRaidItemViewModel(StreamModel stream)
                : this()
            {
                this.Platform = StreamingPlatformTypeEnum.Twitch;
                this.ID = stream.user_id;
                this.Name = stream.user_login;
                this.Viewers = stream.viewer_count;
                this.Category = stream.game_name;
            }

            public ChannelToRaidItemViewModel(StreamModel stream, GameModel game)
                : this()
            {
                this.Platform = StreamingPlatformTypeEnum.Twitch;
                this.ID = stream.user_id;
                this.Name = stream.user_name;
                this.Viewers = stream.viewer_count;
                this.Category = (game != null) ? game.name : MixItUp.Base.Resources.Unknown;
            }

            public ChannelToRaidItemViewModel(TopChannelModel channel)
            {
                this.Platform = StreamingPlatformTypeEnum.Trovo;
                this.ID = channel.channel_id;
                this.Name = channel.username;
                this.Viewers = channel.current_viewers;
                this.Category = channel.category_name;
            }

            private ChannelToRaidItemViewModel()
            {
                this.OpenChannelCommand = this.CreateCommand(() =>
                {
                    ProcessHelper.LaunchLink(this.URL);
                });

                this.RaidChannelCommand = this.CreateCommand(async () =>
                {
                    if (this.Platform == StreamingPlatformTypeEnum.Twitch)
                    {
                        UserModel targetChannel = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetNewAPIUserByLogin(this.Name);
                        if (targetChannel != null)
                        {
                            await ServiceManager.Get<TwitchSessionService>().UserConnection.RaidChannel(ServiceManager.Get<TwitchSessionService>().User, targetChannel);
                        }
                    }
                    else if (this.Platform == StreamingPlatformTypeEnum.Trovo)
                    {
                        await ServiceManager.Get<TrovoChatEventService>().HostUser(this.Name);
                    }
                });
            }

            public string URL
            {
                get
                {
                    if (this.Platform == StreamingPlatformTypeEnum.Twitch)
                    {
                        return $"https://www.twitch.tv/{this.Name}";
                    }
                    else if (this.Platform == StreamingPlatformTypeEnum.Trovo)
                    {
                        return $"https://www.trovo.live/{this.Name}";
                    }
                    return string.Empty;
                }
            }
        }

        public StreamingPlatformTypeEnum Platform { get; protected set; }

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

        public ThreadSafeObservableCollection<string> PastCategories { get; private set; } = new ThreadSafeObservableCollection<string>();

        public string Category
        {
            get { return this.category; }
            set
            {
                this.category = value;
                this.NotifyPropertyChanged();
            }
        }
        private string category;

        public ICommand UpdateChannelInformationCommand { get; private set; }

        public ICommand SearchChannelToRaidCommand { get; private set; }

        public ThreadSafeObservableCollection<ChannelToRaidItemViewModel> ChannelsToRaid { get; private set; } = new ThreadSafeObservableCollection<ChannelToRaidItemViewModel>();

        public PlatformChannelControlViewModelBase()
        {
            this.UpdateChannelInformationCommand = this.CreateCommand(async () =>
            {
                Result result = await this.UpdateChannelInformation();

                await this.RefreshChannelInformation();

                if (!result.Success)
                {
                    Logger.Log(LogLevel.Error, result.ToString());
                    await DialogHelper.ShowFailedResult(result);
                }
            });

            this.SearchChannelToRaidCommand = this.CreateCommand(async () =>
            {
                await this.SearchChannelsToRaid();
            });
        }

        protected override async Task OnOpenInternal()
        {
            this.PastTitles.AddRange(ChannelSession.Settings.RecentStreamTitles);

            this.PastCategories.AddRange(ChannelSession.Settings.RecentStreamCategories);

            await base.OnOpenInternal();
        }

        protected override async Task OnVisibleInternal()
        {
            await this.RefreshChannelInformation();

            await base.OnVisibleInternal();
        }

        protected virtual async Task RefreshChannelInformation()
        {
            this.Title = await StreamingPlatforms.GetPlatformSessionService(this.Platform).GetTitle();
            this.Category = await StreamingPlatforms.GetPlatformSessionService(this.Platform).GetGame();
        }

        protected virtual async Task<Result> UpdateChannelInformation()
        {
            if (!await StreamingPlatforms.GetPlatformSessionService(this.Platform).SetTitle(this.Title) || !await StreamingPlatforms.GetPlatformSessionService(this.Platform).SetGame(this.Category))
            {
                return new Result(MixItUp.Base.Resources.FailedToUpdateChannelInformation);
            }

            ChannelSession.Settings.RecentStreamTitles.Insert(0, this.Title);
            while (ChannelSession.Settings.RecentStreamTitles.Count > 10)
            {
                ChannelSession.Settings.RecentStreamTitles.RemoveAt(ChannelSession.Settings.RecentStreamTitles.Count - 1);
            }
            ChannelSession.Settings.RecentStreamCategories.Insert(0, this.Category);
            while (ChannelSession.Settings.RecentStreamCategories.Count > 10)
            {
                ChannelSession.Settings.RecentStreamCategories.RemoveAt(ChannelSession.Settings.RecentStreamCategories.Count - 1);
            }

            this.PastTitles.ClearAndAddRange(ChannelSession.Settings.RecentStreamTitles);
            this.PastCategories.ClearAndAddRange(ChannelSession.Settings.RecentStreamCategories);

            return new Result();
        }

        protected abstract Task SearchChannelsToRaid();
    }

    public class ChannelMainControlViewModel : WindowControlViewModelBase
    {
        public TwitchChannelControlViewModel Twitch { get; set; } = new TwitchChannelControlViewModel();

        public TrovoChannelControlViewModel Trovo { get; set; } = new TrovoChannelControlViewModel();

        public bool IsTwitchConnected { get { return ServiceManager.Get<TwitchSessionService>().IsConnected; } }

        public bool IsTrovoConnected { get { return ServiceManager.Get<TrovoSessionService>().IsConnected; } }

        public ChannelMainControlViewModel(MainWindowViewModel windowViewModel) : base(windowViewModel) { }

        protected override async Task OnOpenInternal()
        {
            if (this.IsTwitchConnected)
            {
                await this.Twitch.OnOpen();
            }

            if (this.IsTrovoConnected)
            {
                await this.Trovo.OnOpen();
            }

            await base.OnOpenInternal();
        }

        protected override async Task OnVisibleInternal()
        {
            if (this.IsTwitchConnected)
            {
                await this.Twitch.OnVisible();
            }

            if (this.IsTrovoConnected)
            {
                await this.Trovo.OnVisible();
            }

            await base.OnVisibleInternal();
        }
    }
}