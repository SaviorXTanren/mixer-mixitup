using MaterialDesignThemes.Wpf;
using MixItUp.Base.Util;
using MixItUp.WPF.Controls.Dialogs;
using MixItUp.WPF.Windows;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Util
{
    public class WPFDialogShower : IDialogShower
    {
        private static bool isDialogShown = false;

        private static bool lastConfirmationResult = false;
        private static string lastCustomResult = null;

        private static LoadingWindowBase lastActiveWindow;

        public static void SetLastActiveWindow(LoadingWindowBase window)
        {
            WPFDialogShower.lastActiveWindow = window;
        }

        public async Task ShowMessage(string message)
        {
            await this.ShowDialogWrapper(new BasicDialogControl(message));
        }

        public async Task<bool> ShowConfirmation(string message)
        {
            return bool.Equals(await this.ShowDialogWrapper(new ConfirmationDialogControl(message)), true);
        }

        public async Task<string> ShowTextEntry(string message, string defaultValue = null)
        {
            BasicTextEntryDialogControl dialog = new BasicTextEntryDialogControl(message, defaultValue);
            if (bool.Equals(await this.ShowDialogWrapper(dialog), true))
            {
                return dialog.TextEntry;
            }
            return null;
        }

        public async Task<object> ShowCustom(object dialog)
        {
            return await this.ShowDialogWrapper(dialog);
        }

        public void CloseCurrent()
        {
            DialogHost dialogHost = WPFDialogShower.GetActiveWindowDialogHost();
            if (dialogHost != null)
            {
                dialogHost.IsOpen = false;
            }
        }

        private static DialogHost GetActiveWindowDialogHost()
        {
            if (WPFDialogShower.lastActiveWindow != null)
            {
                DialogHost dialog = WPFDialogShower.GetDialogHost(WPFDialogShower.lastActiveWindow);
                if (dialog != null)
                {
                    return dialog;
                }
            }

            IEnumerable<LoadingWindowBase> windows = Application.Current.Windows.OfType<LoadingWindowBase>();
            if (windows.Count() > 0)
            {
                return WPFDialogShower.GetDialogHost(windows.FirstOrDefault(x => x.IsActive));
            }

            return null;
        }

        private static DialogHost GetDialogHost(LoadingWindowBase window)
        {
            if (window != null)
            {
                object obj = window.FindName("MDDialogHost");
                if (obj != null)
                {
                    return (DialogHost)obj;
                }
            }
            return null;
        }

        private static void ConfirmationDialogHost_DialogClosing(object sender, DialogClosingEventArgs eventArgs)
        {
            WPFDialogShower.lastConfirmationResult = bool.Equals(eventArgs.Parameter, true);
        }

        private static void CustomDialogHost_DialogClosing(object sender, DialogClosingEventArgs eventArgs)
        {
            WPFDialogShower.lastCustomResult = (eventArgs.Parameter != null) ? eventArgs.Parameter.ToString() : null;
        }

        private async Task<object> ShowDialogWrapper(object dialog)
        {
            object result = null;
            DialogHost dialogHost = WPFDialogShower.GetActiveWindowDialogHost();
            if (dialogHost != null && !isDialogShown)
            {
                DialogClosingEventHandler onDialogClose = new DialogClosingEventHandler((sender, eventArgs) => { result = eventArgs.Parameter; });

                WPFDialogShower.isDialogShown = true;
                if (onDialogClose != null) { dialogHost.DialogClosing += onDialogClose; }

                try
                {
                    await dialogHost.ShowDialog(dialog);
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }

                if (onDialogClose != null) { dialogHost.DialogClosing -= onDialogClose; }
                WPFDialogShower.isDialogShown = false;
            }
            return result;
        }
    }
}
