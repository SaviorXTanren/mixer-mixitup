using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public abstract class NewServiceControlViewModelBase : UIViewModelBase
    {
        public ICommand ConnectCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand DisableCommand { get; set; }
        public ICommand HelpCommand { get; set; }

        private Task connectTask = null;
        private CancellationTokenSource connectCancellationTokenSource = new CancellationTokenSource();

        private ServiceBase service;

        public NewServiceControlViewModelBase(ServiceBase service)
        {
            this.service = service;

            this.ConnectCommand = this.CreateCommand(() =>
            {
                try
                {
                    this.connectCancellationTokenSource.Cancel();
                    this.connectCancellationTokenSource = new CancellationTokenSource();

                    this.SetServiceValues();

                    this.connectTask = AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
                    {
                        try
                        {
                            Result result = await this.service.ManualConnect(this.connectCancellationTokenSource.Token);
                            if (!result.Success || this.connectCancellationTokenSource.IsCancellationRequested)
                            {
                                await this.service.Disable();

                                string message = Resources.ServiceConnectFailed;
                                if (!string.IsNullOrEmpty(result.Message))
                                {
                                    message += Environment.NewLine + Environment.NewLine + Resources.ServiceConnectAdditionalDetails + " " + result.Message;
                                }
                                await DialogHelper.ShowMessage(message);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(ex);
                        }

                        this.connectTask = null;

                        this.NotifyAllProperties();

                    }, this.connectCancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            });

            this.CancelCommand = this.CreateCommand(() =>
            {
                try
                {
                    this.connectCancellationTokenSource.Cancel();
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }

                this.connectTask = null;

                this.NotifyAllProperties();
            });

            this.DisableCommand = this.CreateCommand(async () =>
            {
                await this.service.Disable();

                this.ResetServiceValues();
            });

            this.HelpCommand = this.CreateCommand(() =>
            {
                ServiceManager.Get<IProcessService>().LaunchLink("https://wiki.mixitupapp.com/services/" + this.WikiPageName);
            });
        }

        public string Name { get { return this.service.Name; } }

        public bool IsDisabled { get { return !this.service.IsEnabled; } }

        public bool IsDisconnected { get { return this.service.IsEnabled && !this.service.IsConnected; } }

        public bool IsConnecting { get { return this.connectTask != null; } }

        public bool IsConnected { get { return this.service.IsConnected; } }

        public virtual string WikiPageName { get { return this.Name; } }

        protected virtual void SetServiceValues() { }

        protected virtual void ResetServiceValues() { }

        protected virtual void NotifyAllProperties()
        {
            this.NotifyPropertyChanged(nameof(this.IsDisabled));
            this.NotifyPropertyChanged(nameof(this.IsDisconnected));
            this.NotifyPropertyChanged(nameof(this.IsConnecting));
            this.NotifyPropertyChanged(nameof(this.IsConnected));
        }
    }
}
