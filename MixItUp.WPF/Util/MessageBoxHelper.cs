using MaterialDesignThemes.Wpf;
using Mixer.Base.Util;
using MixItUp.Base.ViewModel;
using MixItUp.WPF.Controls.Dialogs;
using MixItUp.WPF.Windows;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Util
{
    public enum UserDialogResult
    {
        Purge,
        Timeout1,
        Timeout5,
        Ban,
        Close
    }

    public static class MessageBoxHelper
    {
        private static bool lastConfirmationResult = false;
        private static UserDialogResult lastUserResult = UserDialogResult.Close;

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

            dialogHost.DialogClosing += ConfirmationDialogHost_DialogClosing;
            await dialogHost.ShowDialog(new ConfirmationDialogControl(message));
            dialogHost.DialogClosing -= ConfirmationDialogHost_DialogClosing;

            return MessageBoxHelper.lastConfirmationResult;
        }

        public static async Task<UserDialogResult> ShowUserDialog(UserViewModel user)
        {
            LoadingWindowBase window = Application.Current.Windows.OfType<LoadingWindowBase>().FirstOrDefault(x => x.IsActive);
            DialogHost dialogHost = (DialogHost)window.FindName("MDDialogHost");

            dialogHost.DialogClosing += UserDialogHost_DialogClosing;
            await dialogHost.ShowDialog(new UserDialogControl(user));
            dialogHost.DialogClosing -= UserDialogHost_DialogClosing;

            return MessageBoxHelper.lastUserResult;
        }

        private static void ConfirmationDialogHost_DialogClosing(object sender, DialogClosingEventArgs eventArgs)
        {
            MessageBoxHelper.lastConfirmationResult = bool.Equals(eventArgs.Parameter, true);
        }

        private static void UserDialogHost_DialogClosing(object sender, DialogClosingEventArgs eventArgs)
        {
            MessageBoxHelper.lastUserResult = EnumHelper.GetEnumValueFromString<UserDialogResult>(eventArgs.Parameter.ToString());
        }
    }
}
