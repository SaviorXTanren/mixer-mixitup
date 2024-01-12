using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Overlay.Widgets;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayWidgetV3ViewModel : UIViewModelBase
    {
        public Guid ID
        {
            get { return this.id; }
            set
            {
                this.id = value;
                this.NotifyPropertyChanged();
            }
        }
        private Guid id;

        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                NotifyPropertyChanged();
            }
        }
        private string name;

        public bool IsEnabled
        {
            get { return this.isEnabled; }
            set
            {
                this.isEnabled = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool isEnabled;

        public IEnumerable<OverlayEndpointV3Model> OverlayEndpoints { get; set; } = ServiceManager.Get<OverlayV3Service>().GetOverlayEndpoints();

        public OverlayEndpointV3Model SelectedOverlayEndpoint
        {
            get { return this.selectedOverlayEndpoint; }
            set
            {
                this.selectedOverlayEndpoint = value;
                NotifyPropertyChanged();
            }
        }
        private OverlayEndpointV3Model selectedOverlayEndpoint;

        public int RefreshTime
        {
            get { return this.refreshTime; }
            set
            {
                this.refreshTime = value;
                NotifyPropertyChanged();
            }
        }
        private int refreshTime;

        public OverlayItemV3ViewModelBase Item
        {
            get { return this.item; }
            set
            {
                this.item = value;
                NotifyPropertyChanged();
            }
        }
        private OverlayItemV3ViewModelBase item;

        public OverlayPositionV3ViewModel Position
        {
            get { return this.position; }
            set
            {
                this.position = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayPositionV3ViewModel position = new OverlayPositionV3ViewModel();

        public ObservableCollection<OverlayAnimationV3ViewModel> Animations { get; set; } = new ObservableCollection<OverlayAnimationV3ViewModel>();

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
        public ICommand TestCommand { get; set; }
        public ICommand ExportCommand { get; set; }

        public event EventHandler OnCloseRequested = delegate { };

        private OverlayWidgetV3Model existingWidget;

        public OverlayWidgetV3ViewModel(OverlayItemV3Type type)
        {
            this.ID = Guid.NewGuid();
            this.SelectedOverlayEndpoint = ServiceManager.Get<OverlayV3Service>().GetDefaultOverlayEndpoint();

            switch (type)
            {
                case OverlayItemV3Type.Label:
                    this.Item = new OverlayLabelV3ViewModel();
                    break;
                case OverlayItemV3Type.StreamBoss:
                    this.Item = new OverlayStreamBossV3ViewModel();
                    break;
            }

            this.HTML = OverlayItemV3ModelBase.GetPositionWrappedHTML(this.Item.DefaultHTML);
            this.CSS = OverlayItemV3ModelBase.GetPositionWrappedCSS(this.Item.DefaultCSS);
            this.Javascript = this.Item.DefaultJavascript;

            foreach (OverlayAnimationV3ViewModel animation in this.Item.Animations)
            {
                this.Animations.Add(animation);
            }

            this.SetupCommands();
        }

        public OverlayWidgetV3ViewModel(OverlayWidgetV3Model widget)
        {
            this.existingWidget = widget;

            this.ID = widget.ID;
            this.Name = widget.Name;
            this.RefreshTime = widget.RefreshTime;
            this.SelectedOverlayEndpoint = ServiceManager.Get<OverlayV3Service>().GetOverlayEndpoint(widget.Item.OverlayEndpointID);

            switch (widget.Item.Type)
            {
                case OverlayItemV3Type.Label:
                    this.Item = new OverlayLabelV3ViewModel((OverlayLabelV3Model)widget.Item);
                    break;
                case OverlayItemV3Type.StreamBoss:
                    this.Item = new OverlayStreamBossV3ViewModel((OverlayStreamBossV3Model)widget.Item);
                    break;
            }

            this.HTML = widget.Item.HTML;
            this.CSS = widget.Item.CSS;
            this.Javascript = widget.Item.Javascript;

            this.SetupCommands();
        }

        public Result Validate()
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                return new Result(Resources.ANameMustBeSpecified);
            }

            Result result = this.Item.Validate();
            if (!result.Success)
            {
                return result;
            }

            result = this.Position.Validate();
            if (!result.Success)
            {
                return result;
            }

            return new Result();
        }

        private void SetupCommands()
        {
            this.SaveCommand = this.CreateCommand(async () =>
            {
                Result result = this.Validate();
                if (!result.Success)
                {
                    await DialogHelper.ShowFailedResults(new List<Result>() { result });
                    return;
                }

                if (this.existingWidget != null)
                {
                    await ServiceManager.Get<OverlayV3Service>().RemoveWidget(this.existingWidget);
                }

                OverlayWidgetV3Model widget = this.GetWidget();
                await ServiceManager.Get<OverlayV3Service>().AddWidget(widget);

                this.OnCloseRequested(this, new EventArgs());
            });

            this.TestCommand = this.CreateCommand(async () =>
            {
                Result result = this.Validate();
                if (!result.Success)
                {
                    await DialogHelper.ShowFailedResults(new List<Result>() { result });
                    return;
                }

                CommandParametersModel parameters = CommandParametersModel.GetTestParameters(new Dictionary<string, string>());
                parameters = await DialogHelper.ShowEditTestCommandParametersDialog(parameters);
                if (parameters == null)
                {
                    return;
                }

                //await this.viewModel.Test(parameters);
            });

            this.ExportCommand = this.CreateCommand(async () =>
            {
                Result result = this.Validate();
                if (!result.Success)
                {
                    await DialogHelper.ShowFailedResults(new List<Result>() { result });
                    return;
                }

                OverlayWidgetV3Model widget = this.GetWidget();
            });
        }

        public OverlayWidgetV3Model GetWidget()
        {
            OverlayItemV3ModelBase item = this.Item.GetItem();
            item.ID = this.ID;
            item.OverlayEndpointID = this.SelectedOverlayEndpoint.ID;
            item.HTML = this.HTML;
            item.CSS = this.CSS;
            item.Javascript = this.Javascript;
            item.Position = this.Position.GetPosition();

            OverlayWidgetV3Model widget = new OverlayWidgetV3Model(item);
            widget.Name = this.Name;
            return widget;
        }
    }
}
