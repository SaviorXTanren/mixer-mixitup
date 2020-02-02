using MixItUp.Base.Model;
using MixItUp.Base.ViewModel.Controls.Accounts;
using MixItUp.Base.ViewModel.Window;

namespace MixItUp.Base.ViewModel.Controls.MainControls
{
    public class AccountsMainControlViewModel : WindowControlViewModelBase
    {
        public StreamingPlatformAccountControlViewModel Mixer { get; set; } = new StreamingPlatformAccountControlViewModel(StreamingPlatformTypeEnum.Mixer);

        public StreamingPlatformAccountControlViewModel Twitch { get; set; } = new StreamingPlatformAccountControlViewModel(StreamingPlatformTypeEnum.Twitch);

        public AccountsMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            this.Mixer.StartLoadingOperationOccurred += (sender, eventArgs) => { this.StartLoadingOperation(); };
            this.Mixer.EndLoadingOperationOccurred += (sender, eventArgs) => { this.EndLoadingOperation(); };

            this.Twitch.StartLoadingOperationOccurred += (sender, eventArgs) => { this.StartLoadingOperation(); };
            this.Twitch.EndLoadingOperationOccurred += (sender, eventArgs) => { this.EndLoadingOperation(); };
        }
    }
}
