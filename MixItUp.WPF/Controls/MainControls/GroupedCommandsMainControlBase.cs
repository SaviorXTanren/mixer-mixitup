using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Controls.Commands;
using MixItUp.Base.ViewModel.Controls.MainControls;
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

        protected virtual void Window_CommandSaveSuccessfully(object sender, CommandBase command)
        {
            this.viewModel.RemoveCommand(command);
            this.viewModel.AddCommand(command);
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
