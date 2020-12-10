using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel.Commands;
using MixItUp.Base.ViewModel.MainControls;
using System.Windows;

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
    }
}
