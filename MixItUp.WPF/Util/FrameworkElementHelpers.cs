using System.Windows;

namespace MixItUp.WPF.Util
{
    public static class FrameworkElementHelpers
    {
        public static T GetDataContext<T>(object sender) { return (T)((FrameworkElement)sender).DataContext; }
    }
}
