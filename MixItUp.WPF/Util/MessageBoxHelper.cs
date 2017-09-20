using System.Windows;

namespace MixItUp.WPF.Util
{
    public static class MessageBoxHelper
    {
        public static void ShowError(string message)
        {
            MessageBox.Show(message, "Mix It Up - Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void ShowInformation(string message)
        {
            MessageBox.Show(message, "Mix It Up - Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
