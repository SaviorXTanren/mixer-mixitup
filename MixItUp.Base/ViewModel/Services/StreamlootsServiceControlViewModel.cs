using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using MixItUp.Base.Model.Web;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class StreamlootsServiceControlViewModel : ServiceControlViewModelBase
    {
        private const string StreamlootsStreamURLFormat = "https://widgets.streamloots.com/alerts/";

        public string StreamlootsURL
        {
            get { return this.streamlootsURL; }
            set
            {
                this.streamlootsURL = value;
                this.NotifyPropertyChanged();
            }
        }
        private string streamlootsURL;

        public ICommand LogInCommand { get; set; }
        public ICommand LogOutCommand { get; set; }

        public override string WikiPageName { get { return "streamloots"; } }

        public StreamlootsServiceControlViewModel()
            : base(Resources.Streamloots)
        {
            this.LogInCommand = this.CreateCommand(async () =>
            {
                if (string.IsNullOrEmpty(this.StreamlootsURL) || (!this.StreamlootsURL.StartsWith(StreamlootsStreamURLFormat) && !int.TryParse(this.StreamlootsURL, out int ID)))
                {
                    await DialogHelper.ShowMessage(string.Format(Resources.StreamlootsInvalidUrl, StreamlootsStreamURLFormat));
                }
                else
                {
                    string streamlootsID = this.StreamlootsURL.Replace(StreamlootsStreamURLFormat, "");

                    Result result = await ServiceManager.Get<StreamlootsService>().Connect(new OAuthTokenModel() { accessToken = streamlootsID });
                    if (result.Success)
                    {
                        this.IsConnected = true;
                    }
                    else
                    {
                        await this.ShowConnectFailureMessage(result);
                    }
                }
            });

            this.LogOutCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<StreamlootsService>().Disconnect();

                ChannelSession.Settings.StreamlootsOAuthToken = null;

                this.IsConnected = false;
            });

            this.IsConnected = ServiceManager.Get<StreamlootsService>().IsConnected;
        }
    }
}
