using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using StreamingClient.Base.Util;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayWidgetV3EditorWindowViewModel : UIViewModelBase
    {
        public Guid ID { get; set; }

        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                this.NotifyPropertyChanged();
            }
        }
        private string name;

        public IEnumerable<OverlayItemV3Type> Types { get { return EnumHelper.GetEnumList<OverlayItemV3Type>(); } }

        public OverlayItemV3Type SelectedType
        {
            get { return this.selectedType; }
            set
            {
                this.selectedType = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayItemV3Type selectedType;

        public bool IsTypeSelected
        {
            get { return this.isTypeSelected; }
            set
            {
                this.isTypeSelected = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(IsTypeNotSelected));
            }
        }
        private bool isTypeSelected;

        public bool IsTypeNotSelected { get { return !this.IsTypeSelected; } }

        public IEnumerable<OverlayEndpointV3Model> OverlayEndpoints { get; set; } = ServiceManager.Get<OverlayV3Service>().GetOverlayEndpoints();

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

        public OverlayItemV3ModelBase oldItem;

        public OverlayWidgetV3EditorWindowViewModel()
        {
            this.ID = Guid.NewGuid();
            this.SelectedType = OverlayItemV3Type.Label;
            this.SelectedOverlayEndpoint = ServiceManager.Get<OverlayV3Service>().GetDefaultOverlayEndpoint();
        }

        public OverlayWidgetV3EditorWindowViewModel(OverlayItemV3ModelBase item)
        {
            this.oldItem = item;

            this.IsTypeSelected = true;

            this.ID = item.ID;
            this.Name = item.Name;
            this.SelectedType = item.Type;
            this.RefreshTime = item.RefreshTime;
            this.SelectedOverlayEndpoint = ServiceManager.Get<OverlayV3Service>().GetOverlayEndpoint(item.OverlayEndpointID);

            switch (this.SelectedType)
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

        public async Task TypeSelected()
        {
            switch (this.SelectedType)
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
                case OverlayItemV3Type.Timer:
                    this.Item = new OverlayTimerV3ViewModel();
                    break;
                case OverlayItemV3Type.Label:
                    this.Item = new OverlayLabelV3ViewModel();
                    break;
                case OverlayItemV3Type.EventList:
                    this.Item = new OverlayEventListV3ViewModel();
                    break;
                case OverlayItemV3Type.Goal:
                    this.Item = new OverlayGoalV3ViewModel();
                    break;
            }

            this.IsTypeSelected = true;

            await this.Item.OnOpen();
            await this.Item.OnVisible();
        }

        public Result Validate()
        {
            if (string.IsNullOrWhiteSpace(this.Name))
            {
                return new Result(Resources.NameRequired);
            }

            return this.Item.Validate();
        }

        public async Task Save()
        {
            OverlayItemV3ModelBase item = this.GetItem();
            if (item != null)
            {
                if (this.oldItem != null)
                {
                    //await ServiceManager.Get<OverlayV3Service>().RemoveOverlayWidget(this.oldItem);                
                }
                //await ServiceManager.Get<OverlayV3Service>().AddOverlayWidget(item);
            }
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

        protected override async Task OnOpenInternal()
        {
            if (this.Item != null)
            {
                await this.Item.OnOpen();
            }
        }

        protected override async Task OnVisibleInternal()
        {
            if (this.Item != null)
            {
                await this.Item.OnVisible();
            }
        }

        protected override async Task OnClosedInternal()
        {
            if (this.Item != null)
            {
                await this.Item.OnClosed();
            }
        }
    }
}