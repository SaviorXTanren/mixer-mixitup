using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using StreamingClient.Base.Model.OAuth;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Services
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

        public StreamlootsServiceControlViewModel()
            : base("Streamloots")
        {
            this.LogInCommand = this.CreateCommand(async (parameter) =>
            {
                if (string.IsNullOrEmpty(this.StreamlootsURL) || (!this.StreamlootsURL.StartsWith(StreamlootsStreamURLFormat) && !int.TryParse(this.StreamlootsURL, out int ID)))
                {
                    await DialogHelper.ShowMessage("Please enter a valid Streamloots URL (" + StreamlootsStreamURLFormat + ").");
                }
                else
                {
                    string streamlootsID = this.StreamlootsURL.Replace(StreamlootsStreamURLFormat, "");

                    Result result = await ChannelSession.Services.Streamloots.Connect(new OAuthTokenModel() { accessToken = streamlootsID });
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

            this.LogOutCommand = this.CreateCommand(async (parameter) =>
            {
                await ChannelSession.Services.Streamloots.Disconnect();

                ChannelSession.Settings.StreamlootsOAuthToken = null;

                this.IsConnected = false;
            });

            this.IsConnected = ChannelSession.Services.Streamloots.IsConnected;
        }
    }
}
