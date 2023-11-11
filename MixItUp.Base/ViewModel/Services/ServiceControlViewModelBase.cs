using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public abstract class ServiceControlViewModelBase : UIViewModelBase
    {
        public ICommand HelpCommand { get; set; }

        public ServiceControlViewModelBase(string name)
        {
            this.Name = name;

            this.HelpCommand = this.CreateCommand(() =>
            {
                ServiceManager.Get<IProcessService>().LaunchLink("https://wiki.mixitupapp.com/services/" + this.WikiPageName);
            });
        }

        public string Name { get; private set; }

        public bool IsConnected
        {
            get { return this.isConnected; }
            set
            {
                this.isConnected = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("IsNotConnected");
            }
        }
        private bool isConnected;

        public bool IsNotConnected { get { return !this.IsConnected; } }

        public abstract string WikiPageName { get; }

        protected async Task ShowConnectFailureMessage(Result result)
        {
            string message = Resources.ServiceConnectFailed;
            if (!string.IsNullOrEmpty(result.Message))
            {
                message += Environment.NewLine + Environment.NewLine + Resources.ServiceConnectAdditionalDetails + " " + result.Message;
            }
            await DialogHelper.ShowMessage(message);
        }
    }
}
