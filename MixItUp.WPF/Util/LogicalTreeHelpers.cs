using System.Linq;
using System.Windows;

namespace MixItUp.WPF.Util
{
    public static class LogicalTreeHelpers
    {
        public static UIElement GetByUid(this DependencyObject rootElement, string uid)
        {
            foreach (UIElement element in LogicalTreeHelper.GetChildren(rootElement).OfType<UIElement>())
            {
                if (element.Uid == uid)
                {
                    return element;
                }
                UIElement resultChildren = GetByUid(element, uid);
                if (resultChildren != null)
                {
                    return resultChildren;
                }
            }
            return null;
        }
    }
}
