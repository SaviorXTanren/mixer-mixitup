using MixItUp.Base.Model.Currency;
using MixItUp.Base.ViewModel.Window;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Controls.MainControls
{
    public class RedemptionStoreMainControlViewModel : WindowControlViewModelBase
    {
        public ObservableCollection<StreamPassModel> Purchases { get; private set; } = new ObservableCollection<StreamPassModel>();

        public RedemptionStoreMainControlViewModel(MainWindowViewModel windowViewModel) : base(windowViewModel) { }

        public void Refresh()
        {
            this.Purchases.Clear();
            foreach (StreamPassModel seasonPass in ChannelSession.Settings.StreamPass.Values)
            {
                this.Purchases.Add(seasonPass);
            }
        }

        protected override Task OnLoadedInternal()
        {
            this.Refresh();
            return base.OnVisibleInternal();
        }

        protected override Task OnVisibleInternal()
        {
            this.Refresh();
            return base.OnVisibleInternal();
        }
    }
}
