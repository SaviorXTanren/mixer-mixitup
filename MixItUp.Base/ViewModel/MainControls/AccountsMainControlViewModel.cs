using MixItUp.Base.Model;
using MixItUp.Base.ViewModel.Accounts;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class AccountsMainControlViewModel : WindowControlViewModelBase
    {
        public StreamingPlatformAccountControlViewModel Twitch { get; set; } = new StreamingPlatformAccountControlViewModel(StreamingPlatformTypeEnum.Twitch);

        public StreamingPlatformAccountControlViewModel YouTube { get; set; } = new StreamingPlatformAccountControlViewModel(StreamingPlatformTypeEnum.YouTube);

        public StreamingPlatformAccountControlViewModel Trovo { get; set; } = new StreamingPlatformAccountControlViewModel(StreamingPlatformTypeEnum.Trovo);

        public AccountsMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            this.Twitch.StartLoadingOperationOccurred += (sender, eventArgs) => { this.StartLoadingOperation(); };
            this.Twitch.EndLoadingOperationOccurred += (sender, eventArgs) => { this.EndLoadingOperation(); };
            this.YouTube.StartLoadingOperationOccurred += (sender, eventArgs) => { this.StartLoadingOperation(); };
            this.YouTube.EndLoadingOperationOccurred += (sender, eventArgs) => { this.EndLoadingOperation(); };
            this.Trovo.StartLoadingOperationOccurred += (sender, eventArgs) => { this.StartLoadingOperation(); };
            this.Trovo.EndLoadingOperationOccurred += (sender, eventArgs) => { this.EndLoadingOperation(); };
        }
    }
}
