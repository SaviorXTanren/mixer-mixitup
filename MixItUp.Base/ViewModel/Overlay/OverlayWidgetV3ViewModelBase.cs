using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;

namespace MixItUp.Base.ViewModel.Overlay
{
    public abstract class OverlayWidgetV3ViewModelBase : UIViewModelBase
    {
        public Guid ID { get; set; }

        public string Name { get; set; }

        public Guid OverlayEndpointID { get; set; }

        public IEnumerable<OverlayEndpointV3Model> OverlayEndpoints { get; set; } = ServiceManager.Get<OverlayService>().GetOverlayEndpoints();

        public OverlayEndpointV3Model SelectedOverlayEndpoint
        {
            get { return this.selectedOverlayEndpoint; }
            set
            {
                this.selectedOverlayEndpoint = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayEndpointV3Model selectedOverlayEndpoint;

        public OverlayItemPositionV3ViewModel ItemPosition
        {
            get { return this.Item.ItemPosition; }
            set
            {
                this.Item.ItemPosition = value;
                this.NotifyPropertyChanged();
            }
        }

        public OverlayItemV3ViewModelBase Item
        {
            get { return this.item; }
            set
            {
                this.item = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayItemV3ViewModelBase item;

        public List<OverlayItemAnimationV3ViewModel> Animations { get; private set; } = new List<OverlayItemAnimationV3ViewModel>();

        public OverlayWidgetV3ViewModelBase(OverlayItemV3Type type)
        {
            this.ID = Guid.NewGuid();

            switch (type)
            {
                case OverlayItemV3Type.Text:
                    this.Item = new OverlayTextItemV3ViewModel();
                    break;
                case OverlayItemV3Type.Image:
                    this.Item = new OverlayImageItemV3ViewModel();
                    break;
                case OverlayItemV3Type.Video:
                    this.Item = new OverlayVideoItemV3ViewModel();
                    break;
                case OverlayItemV3Type.YouTube:
                    this.Item = new OverlayYouTubeItemV3ViewModel();
                    break;
                case OverlayItemV3Type.HTML:
                    this.Item = new OverlayHTMLItemV3ViewModel();
                    break;
                case OverlayItemV3Type.WebPage:
                    this.Item = new OverlayWebPageItemV3ViewModel();
                    break;
                case OverlayItemV3Type.Timer:
                    this.Item = new OverlayTimerItemV3ViewModel();
                    break;
                case OverlayItemV3Type.Label:
                    this.Item = new OverlayTextItemV3ViewModel();
                    break;
            }

            this.SelectedOverlayEndpoint = ServiceManager.Get<OverlayService>().GetDefaultOverlayEndpoint();
        }

        public OverlayWidgetV3ViewModelBase(OverlayItemV3ModelBase widget)
        {
            this.ID = widget.ID;
            this.Name = widget.Name;
            this.OverlayEndpointID = widget.OverlayEndpointID;

            switch (widget.Type)
            {
                case OverlayItemV3Type.Text:
                    this.Item = new OverlayTextItemV3ViewModel((OverlayTextItemV3Model)widget);
                    break;
                case OverlayItemV3Type.Image:
                    this.Item = new OverlayImageItemV3ViewModel((OverlayImageItemV3Model)widget);
                    break;
                case OverlayItemV3Type.Video:
                    this.Item = new OverlayVideoItemV3ViewModel((OverlayVideoItemV3Model)widget);
                    break;
                case OverlayItemV3Type.YouTube:
                    this.Item = new OverlayYouTubeItemV3ViewModel((OverlayYouTubeItemV3Model)widget);
                    break;
                case OverlayItemV3Type.HTML:
                    this.Item = new OverlayHTMLItemV3ViewModel((OverlayHTMLItemV3Model)widget);
                    break;
                case OverlayItemV3Type.WebPage:
                    this.Item = new OverlayWebPageItemV3ViewModel((OverlayWebPageItemV3Model)widget);
                    break;
                case OverlayItemV3Type.Timer:
                    this.Item = new OverlayTimerItemV3ViewModel((OverlayTimerItemV3Model)widget);
                    break;
                case OverlayItemV3Type.Label:
                    this.Item = new OverlayTextItemV3ViewModel((OverlayTextItemV3Model)widget);
                    break;
            }

            this.SelectedOverlayEndpoint = ServiceManager.Get<OverlayService>().GetOverlayEndpoint(this.OverlayEndpointID);
        }

        public virtual Result Validate()
        {
            if (string.IsNullOrWhiteSpace(this.Name))
            {
                return new Result(Resources.NameRequired);
            }

            return this.Item.Validate();
        }

        public OverlayItemV3ModelBase GetItem()
        {
            OverlayItemV3ModelBase widget = this.GetItemInternal();

            this.ItemPosition.SetPosition(widget);

            return widget;
        }

        protected abstract OverlayItemV3ModelBase GetItemInternal();
    }
}
