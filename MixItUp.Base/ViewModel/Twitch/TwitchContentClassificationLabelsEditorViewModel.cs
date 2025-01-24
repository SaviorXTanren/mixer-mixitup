using MixItUp.Base.Model.Twitch.Channels;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch.New;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Twitch
{
    public class TwitchContentClassificationLabelViewModel : UIViewModelBase, IEquatable<TwitchContentClassificationLabelViewModel>
    {
        public ChannelContentClassificationLabelModel Label { get; private set; }

        public string ID { get { return this.Label.id; } }

        public string Name { get { return this.Label.name; } }

        public ICommand DeleteLabelCommand { get; private set; }

        public event EventHandler LabelDeleted = delegate { };

        public TwitchContentClassificationLabelViewModel(ChannelContentClassificationLabelModel label)
        {
            this.Label = label;

            this.DeleteLabelCommand = this.CreateCommand(() =>
            {
                this.LabelDeleted.Invoke(this, new EventArgs());
            });
        }

        public bool Equals(TwitchContentClassificationLabelViewModel other) { return this.ID.Equals(other.ID, StringComparison.Ordinal); }
    }

    public class TwitchContentClassificationLabelsEditorViewModel : UIViewModelBase
    {
        public IEnumerable<ChannelContentClassificationLabelModel> AllLabels { get; private set; }

        public ChannelContentClassificationLabelModel SelectedLabel
        {
            get { return this.selectedLabel; }
            set
            {
                this.selectedLabel = value;
                this.NotifyPropertyChanged();
            }
        }
        private ChannelContentClassificationLabelModel selectedLabel;

        public ObservableCollection<TwitchContentClassificationLabelViewModel> Labels { get; private set; } = new ObservableCollection<TwitchContentClassificationLabelViewModel>();

        public ICommand AddLabelCommand { get; private set; }

        public TwitchContentClassificationLabelsEditorViewModel()
        {
            this.AllLabels = ServiceManager.Get<TwitchSession>().ContentClassificationLabels;
        }

        public void AddLabel(string labelID)
        {
            ChannelContentClassificationLabelModel label = this.AllLabels.FirstOrDefault(l => string.Equals(l.id, labelID, StringComparison.Ordinal));
            if (label != null)
            {
                this.AddLabel(label);
            }
        }

        public void AddLabel(ChannelContentClassificationLabelModel label)
        {
            if (label != null)
            {
                TwitchContentClassificationLabelViewModel labelVM = new TwitchContentClassificationLabelViewModel(label);
                if (!this.Labels.Contains(labelVM))
                {
                    this.Labels.Add(labelVM);
                    labelVM.LabelDeleted += (sender, e) =>
                    {
                        this.RemoveLabel((TwitchContentClassificationLabelViewModel)sender);
                    };
                    this.SelectedLabel = null;
                }
            }
        }

        public void ClearCustomTags()
        {
            this.Labels.Clear();
        }

        protected override async Task OnOpenInternal()
        {
            this.AddLabelCommand = this.CreateCommand(() =>
            {
                this.AddLabel(this.SelectedLabel);
            });
            await base.OnOpenInternal();
        }

        private void RemoveLabel(TwitchContentClassificationLabelViewModel label)
        {
            this.Labels.Remove(label);
        }
    }
}
