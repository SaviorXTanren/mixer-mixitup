using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Overlay.Widgets;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Overlay.Widget
{
    public abstract class OverlayWidgetV3ViewModelBase : UIViewModelBase
    {
        public Guid ID { get; set; }

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                NotifyPropertyChanged();
            }
        }
        private string name;

        public IEnumerable<OverlayEndpointV3Model> OverlayEndpoints { get; set; } = ServiceManager.Get<OverlayV3Service>().GetOverlayEndpoints();

        public OverlayEndpointV3Model SelectedOverlayEndpoint
        {
            get { return selectedOverlayEndpoint; }
            set
            {
                selectedOverlayEndpoint = value;
                NotifyPropertyChanged();
            }
        }
        private OverlayEndpointV3Model selectedOverlayEndpoint;

        public int RefreshTime
        {
            get { return refreshTime; }
            set
            {
                refreshTime = value;
                NotifyPropertyChanged();
            }
        }
        private int refreshTime;

        public OverlayItemV3ViewModelBase Item
        {
            get { return item; }
            set
            {
                item = value;
                NotifyPropertyChanged();
            }
        }
        private OverlayItemV3ViewModelBase item;

        public OverlayWidgetV3ViewModelBase()
        {
            this.ID = Guid.NewGuid();
            this.SelectedOverlayEndpoint = ServiceManager.Get<OverlayV3Service>().GetDefaultOverlayEndpoint();
        }

        public OverlayWidgetV3ViewModelBase(OverlayWidgetV3ModelBase widget)
        {
            this.ID = widget.ID;
            this.Name = widget.Name;
            this.RefreshTime = widget.RefreshTime;

            OverlayItemV3ModelBase item = widget.Item;
            this.SelectedOverlayEndpoint = ServiceManager.Get<OverlayV3Service>().GetOverlayEndpoint(widget.OverlayEndpointID);
        }

        public virtual Result Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return new Result(Resources.NameRequired);
            }

            return Item.Validate();
        }

        public abstract OverlayWidgetV3ModelBase GetWidget();

        protected OverlayItemV3ModelBase GetItem()
        {
            OverlayItemV3ModelBase item = this.Item.GetItem();
            item.ID = ID;
            item.Name = Name;
            return item;
        }

        protected override async Task OnOpenInternal()
        {
            if (Item != null)
            {
                await Item.OnOpen();
            }
        }

        protected override async Task OnVisibleInternal()
        {
            if (Item != null)
            {
                await Item.OnVisible();
            }
        }

        protected override async Task OnClosedInternal()
        {
            if (Item != null)
            {
                await Item.OnClosed();
            }
        }
    }
}