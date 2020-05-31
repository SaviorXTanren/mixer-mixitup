using MixItUp.Base.Model.Currency;

namespace MixItUp.Base.ViewModel.Window.Currency
{
    public class StreamPassWindowViewModel : WindowViewModelBase
    {
        private StreamPassModel seasonPass;

        public StreamPassWindowViewModel()
        {

        }

        public StreamPassWindowViewModel(StreamPassModel seasonPass)
            : this()
        {
            this.seasonPass = seasonPass;
        }
    }
}
