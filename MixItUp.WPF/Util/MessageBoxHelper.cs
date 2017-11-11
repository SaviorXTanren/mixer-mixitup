using MaterialDesignThemes.Wpf;
using Mixer.Base.Util;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Controls.Dialogs;
using MixItUp.WPF.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Util
{
    public enum UserDialogResult
    {
        Purge,
        Timeout1,
        Timeout5,
        Ban,
        Unban,
        Close
    }

    public static class MessageBoxHelper
    {
        private static bool isDialogShown = false;

        private static bool lastConfirmationResult = false;
        private static UserDialogResult lastUserResult = UserDialogResult.Close;
        private static string lastCustomResult = null;

        public static async Task ShowMessageDialog(string message)
        {
            LoadingWindowBase window = MessageBoxHelper.GetWindow();
            DialogHost dialogHost = (DialogHost)window.FindName("MDDialogHost");

            if (!isDialogShown)
            {
                MessageBoxHelper.isDialogShown = true;
                await dialogHost.ShowDialog(new BasicDialogControl(message));
                MessageBoxHelper.isDialogShown = false;
            }
        }

        public static async Task<bool> ShowConfirmationDialog(string message)
        {
            LoadingWindowBase window = MessageBoxHelper.GetWindow();
            DialogHost dialogHost = (DialogHost)window.FindName("MDDialogHost");

            if (!isDialogShown)
            {
                MessageBoxHelper.isDialogShown = true;
                dialogHost.DialogClosing += ConfirmationDialogHost_DialogClosing;
                await dialogHost.ShowDialog(new ConfirmationDialogControl(message));
                dialogHost.DialogClosing -= ConfirmationDialogHost_DialogClosing;
                MessageBoxHelper.isDialogShown = false;
            }

            return MessageBoxHelper.lastConfirmationResult;
        }

        public static async Task<UserDialogResult> ShowUserDialog(UserViewModel user)
        {
            LoadingWindowBase window = MessageBoxHelper.GetWindow();
            DialogHost dialogHost = (DialogHost)window.FindName("MDDialogHost");

            if (!isDialogShown)
            {
                MessageBoxHelper.isDialogShown = true;
                dialogHost.DialogClosing += UserDialogHost_DialogClosing;
                await dialogHost.ShowDialog(new UserDialogControl(user));
                dialogHost.DialogClosing -= UserDialogHost_DialogClosing;
                MessageBoxHelper.isDialogShown = false;
            }

            return MessageBoxHelper.lastUserResult;
        }

        public static async Task<string> ShowCustomDialog(UserControl control)
        {
            LoadingWindowBase window = MessageBoxHelper.GetWindow();
            DialogHost dialogHost = (DialogHost)window.FindName("MDDialogHost");

            if (!isDialogShown)
            {
                MessageBoxHelper.isDialogShown = true;
                dialogHost.DialogClosing += CustomDialogHost_DialogClosing;
                await dialogHost.ShowDialog(control);
                dialogHost.DialogClosing -= CustomDialogHost_DialogClosing;
                MessageBoxHelper.isDialogShown = false;
            }

            return MessageBoxHelper.lastCustomResult;
        }

        private static LoadingWindowBase GetWindow()
        {
            IEnumerable<LoadingWindowBase> windows = Application.Current.Windows.OfType<LoadingWindowBase>();
            if (windows.Count() == 1)
            {
                return windows.First();
            }
            else
            {
                return windows.FirstOrDefault(x => x.IsActive);
            }
        }

        private static void ConfirmationDialogHost_DialogClosing(object sender, DialogClosingEventArgs eventArgs)
        {
            MessageBoxHelper.lastConfirmationResult = bool.Equals(eventArgs.Parameter, true);
        }

        private static void UserDialogHost_DialogClosing(object sender, DialogClosingEventArgs eventArgs)
        {
            MessageBoxHelper.lastUserResult = EnumHelper.GetEnumValueFromString<UserDialogResult>(eventArgs.Parameter.ToString());
        }

        private static void CustomDialogHost_DialogClosing(object sender, DialogClosingEventArgs eventArgs)
        {
            MessageBoxHelper.lastCustomResult = (eventArgs.Parameter != null) ? eventArgs.Parameter.ToString() : null;
        }
    }
}
