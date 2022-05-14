using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Twitch
{
    public class TwitchTagViewModel : UIViewModelBase, IEquatable<TwitchTagViewModel>
    {
        public TwitchTagModel Tag
        {
            get { return this.tag; }
            set
            {
                this.tag = value;
                this.NotifyPropertyChanged();
            }
        }
        private TwitchTagModel tag;

        public ICommand DeleteTagCommand { get; private set; }

        public event EventHandler TagDeleted = delegate { };

        public TwitchTagViewModel(TwitchTagModel tag)
        {
            this.Tag = tag;

            this.DeleteTagCommand = this.CreateCommand(() =>
            {
                this.TagDeleted.Invoke(this, new EventArgs());
            });
        }

        public string ID { get { return this.Tag.ID; } }

        public string Name { get { return this.Tag.Name; } }

        public bool IsDeletable { get { return this.Tag.IsDeletable; } }

        public bool Equals(TwitchTagViewModel other) { return this.ID.Equals(other.ID); }
    }

    public class TwitchTagEditorViewModel : UIViewModelBase
    {
        public ThreadSafeObservableCollection<TwitchTagModel> Tags { get; private set; } = new ThreadSafeObservableCollection<TwitchTagModel>();

        public TwitchTagModel SelectedTag
        {
            get { return this.selectedTag; }
            set
            {
                this.selectedTag = value;
                this.NotifyPropertyChanged();
            }
        }
        private TwitchTagModel selectedTag;

        public ThreadSafeObservableCollection<TwitchTagViewModel> CustomTags { get; private set; } = new ThreadSafeObservableCollection<TwitchTagViewModel>();

        public bool CanAddMoreTags { get { return this.CustomTags.Count < 5; } }

        public ICommand AddTagCommand { get; private set; }

        private List<string> existingCustomTags = new List<string>();

        public void SetExistingCustomTags(IEnumerable<string> tags)
        {
            this.existingCustomTags.AddRange(tags);
        }

        public void AddCustomTag(TwitchTagModel tag)
        {
            if (tag != null)
            {
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

        protected override async Task OnOpenInternal()
        {
            this.AddTagCommand = this.CreateCommand(() =>
            {
                this.AddCustomTag(this.SelectedTag);
            });

            if (ServiceManager.Get<TwitchSessionService>().IsConnected)
            {
                this.Tags.ClearAndAddRange(ServiceManager.Get<TwitchSessionService>().StreamTags);
                this.NotifyPropertyChanged("CanAddMoreTags");

                foreach (string tagID in this.existingCustomTags)
                {
                    TwitchTagModel tag = ServiceManager.Get<TwitchSessionService>().StreamTags.FirstOrDefault(t => string.Equals(t.ID, tagID));
                    if (tag != null)
                    {
                        this.AddCustomTag(tag);
                    }
                }
            }
            await base.OnOpenInternal();
        }

        private void RemoveCustomTag(TwitchTagViewModel tag)
        {
            this.CustomTags.Remove(tag);
            this.NotifyPropertyChanged("CanAddMoreTags");
        }
    }
}
