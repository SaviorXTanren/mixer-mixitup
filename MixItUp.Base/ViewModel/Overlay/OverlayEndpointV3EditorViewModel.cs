using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayEndpointV3EditorViewModel : UIViewModelBase
    {
        public event EventHandler OnCloseRequested = delegate { };

        public string Head
        {
            get { return this.head; }
            set
            {
                this.head = value;
                this.NotifyPropertyChanged();
            }
        }
        private string head;

        public string HTML
        {
            get { return this.html; }
            set
            {
                this.html = value;
                this.NotifyPropertyChanged();
            }
        }
        private string html;

        public string CSS
        {
            get { return this.css; }
            set
            {
                this.css = value;
                this.NotifyPropertyChanged();
            }
        }
        private string css;

        public string Javascript
        {
            get { return this.javascript; }
            set
            {
                this.javascript = value;
                this.NotifyPropertyChanged();
            }
        }
        private string javascript;

        public ICommand SaveCommand { get; set; }

        private OverlayEndpointV3Model endpoint;

        public OverlayEndpointV3EditorViewModel(OverlayEndpointV3Model endpoint)
        {
            this.endpoint = endpoint;

            this.Head = endpoint.Head;
            this.HTML = endpoint.HTML;
            this.CSS = endpoint.CSS;
            this.Javascript = endpoint.Javascript;

            this.SaveCommand = this.CreateCommand(() =>
            {
                this.endpoint.Head = this.Head;
                this.endpoint.HTML = this.HTML;
                this.endpoint.CSS = this.CSS;
                this.endpoint.Javascript = this.Javascript;

                OverlayEndpointV3Service endpointService = ServiceManager.Get<OverlayV3Service>().GetOverlayEndpointService(this.endpoint.ID);
                if (endpointService != null)
                {
                    endpointService.RefreshItemIFrameHTMLCache();
                }

                this.OnCloseRequested(this, new EventArgs());
            });
        }

        protected override Task OnClosedInternal()
        {
            return base.OnClosedInternal();
        }
    }
}
