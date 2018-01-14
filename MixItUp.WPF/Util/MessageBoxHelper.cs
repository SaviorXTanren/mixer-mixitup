using MaterialDesignThemes.Wpf;
using Mixer.Base.Util;
using MixItUp.Base.Services;
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
            DialogHost dialogHost = MessageBoxHelper.GetDialogHost();
            if (dialogHost != null && !isDialogShown)
            {
                MessageBoxHelper.isDialogShown = true;
                await dialogHost.ShowDialog(new BasicDialogControl(message));
                MessageBoxHelper.isDialogShown = false;
            }
        }

        public static async Task<bool> ShowConfirmationDialog(string message)
        {
            DialogHost dialogHost = MessageBoxHelper.GetDialogHost();
            if (dialogHost != null && !isDialogShown)
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
            DialogHost dialogHost = MessageBoxHelper.GetDialogHost();
            if (dialogHost != null && !isDialogShown)
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
            DialogHost dialogHost = MessageBoxHelper.GetDialogHost();
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

        public static void CloseDialog()
        {
            DialogHost dialogHost = MessageBoxHelper.GetDialogHost();
            if (dialogHost != null)
            {
                dialogHost.IsOpen = false;
            }
        }

        private static DialogHost GetDialogHost()
        {
            IEnumerable<LoadingWindowBase> windows = Application.Current.Windows.OfType<LoadingWindowBase>();
            if (windows.Count() > 0)
            {
                LoadingWindowBase window = windows.FirstOrDefault(x => x.IsActive);
                if (window != null)
                {
                    object obj = window.FindName("MDDialogHost");
                    if (obj != null)
                    {
                        return (DialogHost)obj;
                    }
                }
            }
            return null;
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
