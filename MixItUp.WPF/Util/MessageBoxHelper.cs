using MaterialDesignThemes.Wpf;
using MixItUp.WPF.Windows;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Util
{
    public static class MessageBoxHelper
    {
        private static bool lastConfirmationResult = false;

        public static async Task ShowMessageDialog(string message)
        {
            LoadingWindowBase window = Application.Current.Windows.OfType<LoadingWindowBase>().FirstOrDefault(x => x.IsActive);
            DialogHost dialogHost = (DialogHost)window.FindName("MDDialogHost");
            await dialogHost.ShowDialog(new BasicDialogControl(message));
        }

        public static async Task<bool> ShowConfirmationDialog(string message)
        {
            LoadingWindowBase window = Application.Current.Windows.OfType<LoadingWindowBase>().FirstOrDefault(x => x.IsActive);
            DialogHost dialogHost = (DialogHost)window.FindName("MDDialogHost");

            dialogHost.DialogClosing += DialogHost_DialogClosing;
            await dialogHost.ShowDialog(new ConfirmationDialogControl(message));
            dialogHost.DialogClosing -= DialogHost_DialogClosing;

            return MessageBoxHelper.lastConfirmationResult;
        }

        private static void DialogHost_DialogClosing(object sender, DialogClosingEventArgs eventArgs)
        {
            MessageBoxHelper.lastConfirmationResult = bool.Equals(eventArgs.Parameter, true);
        }
    }
}
