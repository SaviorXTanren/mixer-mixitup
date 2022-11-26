using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System.Collections.Generic;
using System;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayWidgetV3EditorWindowViewModel : UIViewModelBase
    {
        public Guid ID { get; set; }

        public string Name { get; set; }

        public OverlayItemV3Type Type { get; set; }

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

        public OverlayPositionV3ViewModel Position
        {
            get { return this.Item.Position; }
            set
            {
                this.Item.Position = value;
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

        public int RefreshTime
        {
            get { return this.refreshTime; }
            set
            {
                this.refreshTime = value;
                this.NotifyPropertyChanged();
            }
        }
        private int refreshTime;

        public List<OverlayAnimationV3ViewModel> Animations { get { return this.Item.Animations; } }

        public OverlayItemV3ModelBase oldItem;

        public OverlayWidgetV3EditorWindowViewModel(OverlayItemV3Type type)
        {
            this.ID = Guid.NewGuid();
            this.Type = type;

            switch (type)
            {
                case OverlayItemV3Type.Text:
                    this.Item = new OverlayTextV3ViewModel();
                    break;
                case OverlayItemV3Type.Image:
                    this.Item = new OverlayImageV3ViewModel();
                    break;
                case OverlayItemV3Type.Video:
                    this.Item = new OverlayVideoV3ViewModel();
                    break;
                case OverlayItemV3Type.YouTube:
                    this.Item = new OverlayYouTubeV3ViewModel();
                    break;
                case OverlayItemV3Type.HTML:
                    this.Item = new OverlayHTMLV3ViewModel();
                    break;
                case OverlayItemV3Type.WebPage:
                    this.Item = new OverlayWebPageV3ViewModel();
                    break;
                case OverlayItemV3Type.Timer:
                    this.Item = new OverlayTimerV3ViewModel();
                    break;
                case OverlayItemV3Type.Label:
                    this.Item = new OverlayTextV3ViewModel();
                    break;
                case OverlayItemV3Type.EventList:
                    this.Item = new OverlayEventListV3ViewModel();
                    break;
                case OverlayItemV3Type.Goal:
                    this.Item = new OverlayGoalV3ViewModel();
                    break;
            }

            this.SelectedOverlayEndpoint = ServiceManager.Get<OverlayService>().GetDefaultOverlayEndpoint();
        }

        public OverlayWidgetV3EditorWindowViewModel(OverlayItemV3ModelBase item)
        {
            this.oldItem = item;

            this.ID = item.ID;
            this.Name = item.Name;
            this.Type = item.Type;
            this.RefreshTime = item.RefreshTime;

            this.SelectedOverlayEndpoint = ServiceManager.Get<OverlayService>().GetOverlayEndpoint(item.OverlayEndpointID);

            switch (this.Type)
            {
                case OverlayItemV3Type.Text:
                    this.Item = new OverlayTextV3ViewModel((OverlayTextV3Model)item);
                    break;
                case OverlayItemV3Type.Image:
                    this.Item = new OverlayImageV3ViewModel((OverlayImageV3Model)item);
                    break;
                case OverlayItemV3Type.Video:
                    this.Item = new OverlayVideoV3ViewModel((OverlayVideoV3Model)item);
                    break;
                case OverlayItemV3Type.YouTube:
                    this.Item = new OverlayYouTubeV3ViewModel((OverlayYouTubeV3Model)item);
                    break;
                case OverlayItemV3Type.HTML:
                    this.Item = new OverlayHTMLV3ViewModel((OverlayHTMLV3Model)item);
                    break;
                case OverlayItemV3Type.WebPage:
                    this.Item = new OverlayWebPageV3ViewModel((OverlayWebPageV3Model)item);
                    break;
                case OverlayItemV3Type.Timer:
                    this.Item = new OverlayTimerV3ViewModel((OverlayTimerV3Model)item);
                    break;
                case OverlayItemV3Type.Label:
                    this.Item = new OverlayLabelV3ViewModel((OverlayLabelV3Model)item);
                    break;
                case OverlayItemV3Type.EventList:
                    this.Item = new OverlayEventListV3ViewModel((OverlayEventListV3Model)item);
                    break;
                case OverlayItemV3Type.Goal:
                    this.Item = new OverlayGoalV3ViewModel((OverlayGoalV3Model)item);
                    break;
            }
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
            OverlayItemV3ModelBase item = this.Item.GetItem();

            item.ID = this.ID;
            item.Name = this.Name;
            item.OverlayEndpointID = this.SelectedOverlayEndpoint.ID;
            item.RefreshTime = this.RefreshTime;

            return item;
        }
    }
}