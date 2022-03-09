using MixItUp.Base.Model;
using MixItUp.Base.ViewModels;
using System.Collections.Generic;

namespace MixItUp.Base.ViewModel.Dialogs
{
    public class AddUserDialogControlViewModel : UIViewModelBase
    {
        public IEnumerable<StreamingPlatformTypeEnum> StreamingPlatforms { get { return MixItUp.Base.Model.StreamingPlatforms.SupportedPlatforms; } }

        public StreamingPlatformTypeEnum SelectedStreamingPlatform
        {
            get { return this.selectedStreamingPlatform; }
            set
            {
                this.selectedStreamingPlatform = value;
                this.NotifyPropertyChanged();
            }
        }
        private StreamingPlatformTypeEnum selectedStreamingPlatform = ChannelSession.Settings.DefaultStreamingPlatform;

        public string Username
        {
            get { return this.username; }
            set
            {
                this.username = value;
                this.NotifyPropertyChanged();
            }
        }
        private string username;
    }
}
