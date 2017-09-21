using MaterialDesignThemes.Wpf;

namespace MixItUp.WPF.Util
{
    public static class MessageBoxHelper
    {
        public static void ShowDialog(string message) { DialogHost.Show(new BasicDialogControl(message), "RootDialog"); }
    }
}
