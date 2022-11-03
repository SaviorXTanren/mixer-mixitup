using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.MainControls;
using MixItUp.Base.ViewModel;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Overlay;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System;
using MixItUp.Base.Services;

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

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                OverlayWidgetModel widget = FrameworkElementHelpers.GetDataContext<OverlayWidgetModel>(sender);
                await this.viewModel.PlayWidget(widget);
            });
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            OverlayWidgetModel widget = FrameworkElementHelpers.GetDataContext<OverlayWidgetModel>(sender);
            if (widget != null)
            {
                OverlayWidgetEditorWindow window = new OverlayWidgetEditorWindow(widget);
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
                    OverlayWidgetModel widget = FrameworkElementHelpers.GetDataContext<OverlayWidgetModel>(sender);
                    await this.viewModel.DeleteWidget(widget);
                    await this.viewModel.OnVisible();
                }
            });
        }

        private async void EnableDisableToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            await this.viewModel.EnableWidget(FrameworkElementHelpers.GetDataContext<OverlayWidgetModel>(sender));
        }

        private async void EnableDisableToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            await this.viewModel.DisableWidget(FrameworkElementHelpers.GetDataContext<OverlayWidgetModel>(sender));
        }

        private async void AddOverlayWidgetButton_Click(object sender, RoutedEventArgs e)
        {
            OverlayLabelWidgetV3Model widget = new OverlayLabelWidgetV3Model(OverlayItemV3ModelBase.GenerateOverlayItemID(), "Test", Guid.Empty, new OverlayTextItemV3Model()
            {
                CSS = OverlayLabelWidgetV3Model.DefaultCSS,
                FontColor = "Red",
                FontName = "Arial",
                FontSize = 100,
                Javascript = OverlayLabelWidgetV3Model.DefaultJavascript,
                HTML = OverlayLabelWidgetV3Model.DefaultAmountHTML,
                Type = OverlayItemV3Type.Label,
                XPosition = 50,
                YPosition = 50,
                IsPercentagePosition = true
            })
            {
                LabelType = OverlayLabelWidgetV3Type.Counter,
            };

            await widget.Enable();

            //OverlayWidgetEditorWindow window = new OverlayWidgetEditorWindow();
            //window.Closed += Window_Closed;
            //window.Show();
        }
    }
}
