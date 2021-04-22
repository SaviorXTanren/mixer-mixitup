using MixItUp.Base.Model;
using MixItUp.Base.ViewModel.Accounts;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class AccountsMainControlViewModel : WindowControlViewModelBase
    {
        public StreamingPlatformAccountControlViewModel Twitch { get; set; } = new StreamingPlatformAccountControlViewModel(StreamingPlatformTypeEnum.Twitch);

        public AccountsMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            this.Twitch.StartLoadingOperationOccurred += (sender, eventArgs) => { this.StartLoadingOperation(); };
            this.Twitch.EndLoadingOperationOccurred += (sender, eventArgs) => { this.EndLoadingOperation(); };
        }
    }
}
