using MixItUp.Base.Model.Currency;
using MixItUp.Base.Util;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class StreamPassMainControlViewModel : WindowControlViewModelBase
    {
        public ObservableCollection<StreamPassModel> StreamPasses { get; private set; } = new ObservableCollection<StreamPassModel>();

        public StreamPassMainControlViewModel(MainWindowViewModel windowViewModel) : base(windowViewModel) { }

        public void Refresh()
        {
            this.StreamPasses.ClearAndAddRange(ChannelSession.Settings.StreamPass.Values.ToList());
        }

        public async Task Copy(StreamPassModel streamPass)
        {
            StreamPassModel newStreamPass = new StreamPassModel(streamPass);
            ChannelSession.Settings.StreamPass[newStreamPass.ID] = newStreamPass;
            await ChannelSession.SaveSettings();
            this.Refresh();
        }

        public async Task Delete(StreamPassModel streamPass)
        {
            if (await DialogHelper.ShowConfirmation(MixItUp.Base.Resources.ConfirmStreamPassDeletion))
            {
                ChannelSession.Settings.StreamPass.Remove(streamPass.ID);
                await ChannelSession.SaveSettings();
                this.Refresh();
            }
        }

        protected override Task OnOpenInternal()
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
