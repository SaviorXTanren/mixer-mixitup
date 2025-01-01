using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Overlay.Widgets;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel;
using MixItUp.Base.ViewModel.MainControls;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Overlay;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for OverlayWidgetsControl.xaml
    /// </summary>
    public partial class OverlayWidgetsControl : MainControlBase
    {
        private OverlayWidgetsMainControlViewModel viewModel;

        public OverlayWidgetsControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new OverlayWidgetsMainControlViewModel((MainWindowViewModel)this.Window.ViewModel);
            await this.viewModel.OnOpen();
            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.viewModel.OnVisible();
        }

        private async void Window_Closed(object sender, System.EventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                await this.viewModel.OnVisible();
                return Task.CompletedTask;
            });
        }

        private void LinkButton_Click(object sender, RoutedEventArgs e)
        {
            OverlayWidgetViewModel widget = FrameworkElementHelpers.GetDataContext<OverlayWidgetViewModel>(sender);
            string url = widget.SingleWidgetURL;
            if (url != null)
            {
                ServiceManager.Get<IProcessService>().LaunchLink(url);
            }
        }

        private async void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                if (await DialogHelper.ShowConfirmation(MixItUp.Base.Resources.OverlayWidgetResetConfirmation))
                {
                    OverlayWidgetViewModel widget = FrameworkElementHelpers.GetDataContext<OverlayWidgetViewModel>(sender);
                    await widget.Reset();
                }
            });
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            OverlayWidgetViewModel widget = FrameworkElementHelpers.GetDataContext<OverlayWidgetViewModel>(sender);
            if (widget != null)
            {
                OverlayWidgetV3EditorWindow window = new OverlayWidgetV3EditorWindow(widget.Widget);
                window.Closed += Window_Closed;
                window.Show();
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                if (await DialogHelper.ShowConfirmation(MixItUp.Base.Resources.DeleteWidgetPrompt))
                {
                    OverlayWidgetViewModel widget = FrameworkElementHelpers.GetDataContext<OverlayWidgetViewModel>(sender);
                    await this.viewModel.DeleteWidget(widget);
                    await this.viewModel.OnVisible();
                }
            });
        }

        private async void EnableDisableToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            await this.viewModel.EnableWidget(FrameworkElementHelpers.GetDataContext<OverlayWidgetViewModel>(sender));
        }

        private async void EnableDisableToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            await this.viewModel.DisableWidget(FrameworkElementHelpers.GetDataContext<OverlayWidgetViewModel>(sender));
        }

        private async void AddOverlayWidgetButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                List<OverlayItemV3Type> widgetTypes = new List<OverlayItemV3Type>();
                foreach (OverlayItemV3Type value in EnumHelper.GetEnumList<OverlayItemV3Type>())
                {
                    var attributes = (OverlayWidgetAttribute[])value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(OverlayWidgetAttribute), false);
                    if (attributes != null && attributes.Length > 0)
                    {
                        widgetTypes.Add(value);
                    }
                }

                object result = await DialogHelper.ShowEnumDropDown(widgetTypes, MixItUp.Base.Resources.OverlayWidgetSelectorDescription);
                if (result != null)
                {
                    OverlayItemV3Type type = (OverlayItemV3Type)result;
                    OverlayWidgetV3EditorWindow window = new OverlayWidgetV3EditorWindow(type);
                    window.Closed += Window_Closed;
                    window.Show();

                    await Task.Delay(500);
                    window.Focus();
                }
            });
        }

        private async void ImportOverlayWidgetButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string filepath = ServiceManager.Get<IFileService>().ShowOpenFileDialog(MixItUp.Base.Resources.MixItUpOverlayFileFormatFilter);
                if (!string.IsNullOrWhiteSpace(filepath))
                {
                    OverlayWidgetV3Model widget = await FileSerializerHelper.DeserializeFromFile<OverlayWidgetV3Model>(filepath);
                    if (widget != null)
                    {
                        widget.Item.ImportReset();
                        OverlayWidgetV3EditorWindow window = new OverlayWidgetV3EditorWindow(widget);
                        window.Closed += Window_Closed;
                        window.Show();
                        window.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                await DialogHelper.ShowMessage(MixItUp.Base.Resources.FailedToImportOverlayWidget + ": " + ex.ToString());
            }
        }
    }
}
