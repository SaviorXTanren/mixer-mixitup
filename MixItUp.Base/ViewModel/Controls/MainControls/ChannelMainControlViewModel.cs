using MixItUp.Base.ViewModel.Window;
using MixItUp.Base.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Twitch.Base.Models.NewAPI.Channels;
using Twitch.Base.Models.NewAPI.Games;
using Twitch.Base.Models.NewAPI.Tags;

namespace MixItUp.Base.ViewModel.Controls.MainControls
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

            this.DeleteTagCommand = this.CreateCommand((parameter) =>
            {
                this.viewModel.RemoveTag(this);
                return Task.FromResult(0);
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

    public class ChannelMainControlViewModel : WindowControlViewModelBase
    {
        public ObservableCollection<string> PastTitles { get; private set; } = new ObservableCollection<string>();

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

        public ObservableCollection<GameModel> Games { get; private set; } = new ObservableCollection<GameModel>();

        public GameModel Game
        {
            get { return this.game; }
            set
            {
                this.game = value;
                this.NotifyPropertyChanged();
            }
        }
        private GameModel game;

        public ObservableCollection<TagViewModel> Tags { get; private set; } = new ObservableCollection<TagViewModel>();

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

        public ObservableCollection<TagViewModel> CustomTags { get; private set; } = new ObservableCollection<TagViewModel>();

        public ChannelInformationModel ChannelInformation { get; private set; }

        public ICommand AddTagCommand { get; private set; }

        public bool CanAddMoreTags { get { return this.CustomTags.Count < 5; } }

        public ICommand UpdateChannelInformationCommand { get; private set; }

        public ChannelMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            this.AddTagCommand = this.CreateCommand((parameter) =>
            {
                if (this.Tag != null && !this.CustomTags.Contains(tag))
                {
                    this.CustomTags.Add(tag);
                    this.Tag = null;
                }
                this.NotifyPropertyChanged("CanAddMoreTags");
                return Task.FromResult(0);
            });

            this.UpdateChannelInformationCommand = this.CreateCommand(async (parameter) =>
            {
                await ChannelSession.TwitchUserConnection.UpdateChannelInformation(ChannelSession.TwitchUserNewAPI, this.Title, this.Game?.id);

                IEnumerable<TagModel> tags = this.CustomTags.Select(t => t.Tag);
                await ChannelSession.TwitchUserConnection.UpdateStreamTagsForChannel(ChannelSession.TwitchUserNewAPI, tags);

                await this.RefreshChannelInformation();
            });
        }

        public async Task<bool> SetSearchGamesForName(string name)
        {
            this.Games.Clear();
            if (!string.IsNullOrEmpty(name) && name.Length > 3)
            {
                IEnumerable<GameModel> games = await ChannelSession.TwitchUserConnection.GetNewAPIGamesByName(name);
                if (games != null && games.Count() > 0)
                {
                    foreach (GameModel game in games)
                    {
                        this.Games.Add(game);
                        return true;
                    }
                }
            }
            return false;
        }

        public void RemoveTag(TagViewModel tag)
        {
            this.CustomTags.Remove(tag);
            this.NotifyPropertyChanged("CanAddMoreTags");
        }

        protected override async Task OnLoadedInternal()
        {
            foreach (string title in ChannelSession.Settings.RecentStreamTitles)
            {
                this.PastTitles.Add(title);
            }

            List<TagViewModel> tags = new List<TagViewModel>();
            foreach (TagModel tag in await ChannelSession.TwitchUserConnection.GetStreamTags())
            {
                if (!tag.is_auto)
                {
                    tags.Add(new TagViewModel(this, tag));
                }
            }

            this.Tags.Clear();
            foreach (TagViewModel tag in tags.OrderBy(t => t.Name))
            {
                this.Tags.Add(tag);
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
            this.ChannelInformation = await ChannelSession.TwitchUserConnection.GetChannelInformation(ChannelSession.TwitchUserNewAPI);
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
                    this.Game = new GameModel()
                    {
                        id = this.ChannelInformation.game_id,
                        name = this.ChannelInformation.game_name
                    };
                }
            }

            this.CustomTags.Clear();
            foreach (TagModel tag in await ChannelSession.TwitchUserConnection.GetStreamTagsForChannel(ChannelSession.TwitchUserNewAPI))
            {
                if (!tag.is_auto)
                {
                    TagViewModel tagViewModel = this.Tags.FirstOrDefault(t => string.Equals(t.ID, tag.tag_id));
                    if (tagViewModel != null)
                    {
                        this.CustomTags.Add(tagViewModel);
                    }
                }
            }
            this.NotifyPropertyChanged("CanAddMoreTags");
        }
    }
}
