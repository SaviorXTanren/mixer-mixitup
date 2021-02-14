using System.Windows;

namespace MixItUp.WPF.Util
{
    public static class FrameworkElementHelpers
    {
        public static T GetDataContext<T>(object sender)
        {
            object dc = ((FrameworkElement)sender).DataContext;
            if (dc is T)
            {
                return (T)dc;
            }
            return default(T);
        }
    }
}
