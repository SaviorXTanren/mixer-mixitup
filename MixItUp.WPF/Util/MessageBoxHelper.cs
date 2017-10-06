using MaterialDesignThemes.Wpf;
using MixItUp.WPF.Windows;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Util
{
    public static class MessageBoxHelper
    {
        public static async Task ShowDialog(string message)
        {
            LoadingWindowBase window = Application.Current.Windows.OfType<LoadingWindowBase>().FirstOrDefault(x => x.IsActive);
            DialogHost dialogHost = (DialogHost)window.FindName("MDDialogHost");
            await dialogHost.ShowDialog(new BasicDialogControl(message));
        }
    }
}
