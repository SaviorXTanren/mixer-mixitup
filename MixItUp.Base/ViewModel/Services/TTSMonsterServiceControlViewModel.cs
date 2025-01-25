using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using MixItUp.Base.Model.Web;
using System;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class TTSMonsterServiceControlViewModel : ServiceControlViewModelBase
    {
        private const string TTSMonsterOverlayURLFormat = "https://tts.monster/overlay/";

        public string TTSMonsterURL
        {
            get { return this.ttsMonsterURL; }
            set
            {
                this.ttsMonsterURL = value;
                this.NotifyPropertyChanged();
            }
        }
        private string ttsMonsterURL;

        public ICommand LogInCommand { get; set; }
        public ICommand LogOutCommand { get; set; }

        public override string WikiPageName { get { return "ttsmonster"; } }

        public TTSMonsterServiceControlViewModel()
            : base(Resources.TTSMonster)
        {
            this.LogInCommand = this.CreateCommand(async () =>
            {
                if (!string.IsNullOrEmpty(this.TTSMonsterURL) && this.TTSMonsterURL.StartsWith(TTSMonsterOverlayURLFormat))
                {
                    string overlayURL = this.TTSMonsterURL.Replace(TTSMonsterOverlayURLFormat, "");
                    string[] splits = overlayURL.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    if (splits != null && splits.Length == 2)
                    {
                        Result result = await ServiceManager.Get<ITTSMonsterService>().Connect(new OAuthTokenModel()
                        {
                            clientID = splits[0],
                            accessToken = splits[1]
                        });

                        if (result.Success)
                        {
                            this.IsConnected = true;
                            return;
                        }
                        else
                        {
                            await this.ShowConnectFailureMessage(result);
                            return;
                        }
                    }
                }

                await DialogHelper.ShowMessage(string.Format(Resources.TTSMonsterInvalidURL, TTSMonsterOverlayURLFormat));
            });

            this.LogOutCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<ITTSMonsterService>().Disconnect();

                ChannelSession.Settings.TTSMonsterOAuthToken = null;

                this.IsConnected = false;
            });

            this.IsConnected = ServiceManager.Get<ITTSMonsterService>().IsConnected;
        }
    }
}
