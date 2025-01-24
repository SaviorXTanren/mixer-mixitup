using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Services.Twitch.New;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Twitch
{
    public class TwitchTagViewModel : UIViewModelBase, IEquatable<TwitchTagViewModel>
    {
        public string Tag
        {
            get { return this.tag; }
            set
            {
                this.tag = value;
                this.NotifyPropertyChanged();
            }
        }
        private string tag;

        public ICommand DeleteTagCommand { get; private set; }

        public event EventHandler TagDeleted = delegate { };

        public TwitchTagViewModel(string tag)
        {
            this.Tag = tag;

            this.DeleteTagCommand = this.CreateCommand(() =>
            {
                this.TagDeleted.Invoke(this, new EventArgs());
            });
        }

        public bool Equals(TwitchTagViewModel other) { return this.Tag.Equals(other.Tag, StringComparison.Ordinal); }
    }

    public class TwitchTagEditorViewModel : UIViewModelBase
    {
        public string SelectedTag
        {
            get { return this.selectedTag; }
            set
            {
                this.selectedTag = value;
                this.NotifyPropertyChanged();
            }
        }
        private string selectedTag;

        public ObservableCollection<TwitchTagViewModel> CustomTags { get; private set; } = new ObservableCollection<TwitchTagViewModel>();

        public bool CanAddMoreTags { get { return this.CustomTags.Count < 10; } }

        public ICommand AddTagCommand { get; private set; }

        public async Task AddCustomTag(string tag)
        {
            if (tag != null)
            {
                if (tag.Length > 25 || tag.Any(c => !char.IsLetterOrDigit(c)))
                {
                    await DialogHelper.ShowMessage(Resources.TwitchCustomTagsMustMeetStandard);
                    return;
                }

                TwitchTagViewModel tagVM = new TwitchTagViewModel(tag);
                if (!this.CustomTags.Contains(tagVM))
                {
                    this.CustomTags.Add(tagVM);
                    tagVM.TagDeleted += (sender, e) =>
                    {
                        this.RemoveCustomTag((TwitchTagViewModel)sender);
                    };
                    this.SelectedTag = null;
                }
            }
            this.NotifyPropertyChanged("CanAddMoreTags");
        }

        public void ClearCustomTags()
        {
            this.CustomTags.Clear();
            this.NotifyPropertyChanged("CanAddMoreTags");
        }

        public async Task LoadCurrentTags()
        {
            if (ServiceManager.Get<TwitchSession>().IsConnected && ServiceManager.Get<TwitchSession>().Channel?.tags != null)
            {
                this.CustomTags.Clear();
                this.NotifyPropertyChanged("CanAddMoreTags");

                foreach (string tag in ServiceManager.Get<TwitchSession>().Channel.tags)
                {
                    await this.AddCustomTag(tag);
                }
            }
        }

        protected override async Task OnOpenInternal()
        {
            this.AddTagCommand = this.CreateCommand(async () =>
            {
                await this.AddCustomTag(this.SelectedTag);
            });
            await base.OnOpenInternal();
        }

        private void RemoveCustomTag(TwitchTagViewModel tag)
        {
            this.CustomTags.Remove(tag);
            this.NotifyPropertyChanged("CanAddMoreTags");
        }
    }
}
