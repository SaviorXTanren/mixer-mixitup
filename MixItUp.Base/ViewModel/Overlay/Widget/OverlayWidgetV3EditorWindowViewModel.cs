using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Overlay.Widgets;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Overlay.Widget
{
    public class OverlayWidgetV3EditorWindowViewModel : UIViewModelBase
    {
        private static readonly HashSet<OverlayItemV3Type> NonWidgetTypes = new HashSet<OverlayItemV3Type>() { OverlayItemV3Type.TwitchClip };

        public IEnumerable<OverlayItemV3Type> Types { get; } = EnumHelper.GetEnumList<OverlayItemV3Type>().Where(t => !NonWidgetTypes.Contains(t));

        public OverlayItemV3Type SelectedType
        {
            get { return selectedType; }
            set
            {
                selectedType = value;
                NotifyPropertyChanged();
            }
        }
        private OverlayItemV3Type selectedType;

        public bool IsTypeSelected
        {
            get { return isTypeSelected; }
            set
            {
                isTypeSelected = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(IsTypeNotSelected));
            }
        }
        private bool isTypeSelected;

        public bool IsTypeNotSelected { get { return !IsTypeSelected; } }

        public OverlayWidgetV3ViewModelBase WidgetViewModel
        {
            get { return this.widgetViewModel; }
            set
            {
                this.widgetViewModel = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayWidgetV3ViewModelBase widgetViewModel;

        private OverlayWidgetV3ModelBase oldWidget;

        public OverlayWidgetV3EditorWindowViewModel()
        {
            this.SelectedType = OverlayItemV3Type.Label;
        }

        public OverlayWidgetV3EditorWindowViewModel(OverlayWidgetV3ModelBase widget)
        {
            this.oldWidget = widget;

            this.IsTypeSelected = true;

            OverlayItemV3ModelBase item = widget.Item;
            this.SelectedType = item.Type;
            switch (this.SelectedType)
            {
                //case OverlayItemV3Type.Text:
                //    this.WidgetViewModel.WidgetViewModel = new OverlayTextV3ViewModel((OverlayTextV3Model)item); break;
                //case OverlayItemV3Type.Image:
                //    this.WidgetViewModel = new OverlayImageV3ViewModel((OverlayImageV3Model)item); break;
                //case OverlayItemV3Type.Video:
                //    this.WidgetViewModel = new OverlayVideoV3ViewModel((OverlayVideoV3Model)item); break;
                //case OverlayItemV3Type.YouTube:
                //    this.WidgetViewModel = new OverlayYouTubeV3ViewModel((OverlayYouTubeV3Model)item); break;
                //case OverlayItemV3Type.HTML:
                //    this.WidgetViewModel = new OverlayHTMLV3ViewModel((OverlayHTMLV3Model)item); break;
                //case OverlayItemV3Type.Timer:
                //    this.WidgetViewModel = new OverlayTimerV3ViewModel((OverlayTimerV3Model)item); break;
                //case OverlayItemV3Type.Label:
                //    this.WidgetViewModel = new OverlayLabelWidgetV3ViewModel((OverlayLabelWidgetV3Model)widget); break;
            }
        }

        public async Task TypeSelected()
        {
            switch (this.SelectedType)
            {
                //case OverlayItemV3Type.Text:
                //    this.WidgetViewModel = new OverlayTextV3ViewModel(); break;
                //case OverlayItemV3Type.Image:
                //    this.WidgetViewModel = new OverlayImageV3ViewModel(); break;
                //case OverlayItemV3Type.Video:
                //    this.WidgetViewModel = new OverlayVideoV3ViewModel(); break;
                //case OverlayItemV3Type.YouTube:
                //    this.WidgetViewModel = new OverlayYouTubeV3ViewModel(); break;
                //case OverlayItemV3Type.HTML:
                //    this.WidgetViewModel = new OverlayHTMLV3ViewModel(); break;
                //case OverlayItemV3Type.Timer:
                //    this.WidgetViewModel = new OverlayTimerV3ViewModel(); break;
                //case OverlayItemV3Type.Label:
                //    this.WidgetViewModel = new OverlayLabelWidgetV3ViewModel(); break;
            }

            this.IsTypeSelected = true;

            await this.WidgetViewModel.OnOpen();
            await this.WidgetViewModel.OnVisible();
        }

        public Result Validate() { return this.WidgetViewModel.Validate(); }

        public async Task Save()
        {
            OverlayWidgetV3ModelBase widget = this.WidgetViewModel.GetWidget();
            if (widget != null)
            {
                if (oldWidget != null)
                {
                    //await ServiceManager.Get<OverlayV3Service>().RemoveWidget(oldWidget);
                }
                //await ServiceManager.Get<OverlayV3Service>().AddWidget(widget);
            }
        }

        public async Task Test(CommandParametersModel parameters)
        {
            OverlayWidgetV3ModelBase widget = this.WidgetViewModel.GetWidget();
            if (widget != null && widget.IsTestable)
            {
                await widget.Test(parameters);
            }
        }

        protected override async Task OnOpenInternal()
        {
            if (this.WidgetViewModel != null)
            {
                await this.WidgetViewModel.OnOpen();
            }
        }

        protected override async Task OnVisibleInternal()
        {
            if (this.WidgetViewModel != null)
            {
                await this.WidgetViewModel.OnVisible();
            }
        }

        protected override async Task OnClosedInternal()
        {
            if (this.WidgetViewModel != null)
            {
                await this.WidgetViewModel.OnClosed();
            }
        }
    }
}