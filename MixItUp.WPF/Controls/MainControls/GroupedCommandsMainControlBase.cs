using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel.Commands;
using MixItUp.Base.ViewModel.MainControls;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MixItUp.WPF.Controls.MainControls
{
    public class GroupedCommandsMainControlBase : MainControlBase
    {
        private GroupedCommandsMainControlViewModelBase viewModel;

        protected void SetViewModel(GroupedCommandsMainControlViewModelBase viewModel)
        {
            this.viewModel = viewModel;
        }

        protected void Window_CommandSaved(object sender, CommandModelBase e)
        {
            this.viewModel.AddCommand(e);
        }

        protected override async Task OnVisibilityChanged()
        {
            this.viewModel.ClearNameFilter();

            await this.viewModel.OnVisible();

            await base.OnVisibilityChanged();
        }

        protected void AccordianGroupBoxControl_Minimized(object sender, RoutedEventArgs e)
        {
            AccordianGroupBoxControl control = (AccordianGroupBoxControl)sender;
            if (control.Content != null)
            {
                FrameworkElement content = (FrameworkElement)control.Content;
                if (content != null)
                {
                    CommandGroupControlViewModel group = (CommandGroupControlViewModel)content.DataContext;
                    if (group != null)
                    {
                        group.IsMinimized = true;
                    }
                }
            }
        }

        protected void AccordianGroupBoxControl_Maximized(object sender, RoutedEventArgs e)
        {
            AccordianGroupBoxControl control = (AccordianGroupBoxControl)sender;
            if (control.Content != null)
            {
                FrameworkElement content = (FrameworkElement)control.Content;
                if (content != null)
                {
                    CommandGroupControlViewModel group = (CommandGroupControlViewModel)content.DataContext;
                    if (group != null)
                    {
                        group.IsMinimized = false;
                    }
                }
            }
        }

        protected void DataGrid_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }
    }
}
