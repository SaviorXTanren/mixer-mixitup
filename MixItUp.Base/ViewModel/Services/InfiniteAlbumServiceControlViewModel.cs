using MixItUp.Base.Model.Web;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class InfiniteAlbumServiceControlViewModel : ServiceControlViewModelBase
    {
        public ICommand ConnectCommand { get; set; }
        public ICommand DisconnectCommand { get; set; }

        public override string WikiPageName { get { return "infinite-album"; } }

        public string AppCode { get; set; }

        public InfiniteAlbumServiceControlViewModel()
            : base(Resources.InfiniteAlbum)
        {
            this.ConnectCommand = this.CreateCommand(async () =>
            {
                ChannelSession.Settings.InfiniteAlbumOAuthToken = new OAuthTokenModel
                {
                    refreshToken = this.AppCode
                };

                Result result = await ServiceManager.Get<InfiniteAlbumService>().Connect(ChannelSession.Settings.InfiniteAlbumOAuthToken);
                if (result.Success)
                {
                    this.IsConnected = true;
                }
                else
                {
                    await this.ShowConnectFailureMessage(result);
                }
            });

            this.DisconnectCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<InfiniteAlbumService>().Disconnect();

                ChannelSession.Settings.InfiniteAlbumOAuthToken = null;

                this.IsConnected = false;
            });

            this.IsConnected = ServiceManager.Get<InfiniteAlbumService>().IsConnected;
        }
    }
}
