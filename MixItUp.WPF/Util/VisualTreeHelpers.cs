using System.Windows;
using System.Windows.Media;

namespace MixItUp.WPF.Util
{
    public static class VisualTreeHelpers
    {
        public static T GetVisualChild<T>(UIElement parent) where T : UIElement
        {
            T child = null; // default(T);

            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                UIElement element = (UIElement)VisualTreeHelper.GetChild(parent, i);
                child = element as T;

                if (child == null)
                {
                    child = GetVisualChild<T>(element);
                }

                if (child != null)
                {
                    break;
                }
            }

            return child;
        }

        public static T GetVisualParentByName<T>(UIElement child, string name) where T : UIElement
        {
            var parent = VisualTreeHelper.GetParent(child);
            if (parent != null && parent is UIElement)
            {
                if (parent is FrameworkElement)
                {
                    FrameworkElement element = parent as FrameworkElement;
                    if (!string.IsNullOrEmpty(element.Name) && element.Name.Equals(name))
                    {
                        return element as T;
                    }
                }
                return VisualTreeHelpers.GetVisualParentByName<T>(parent as UIElement, name);
            }
            return null;
        }

        public static T GetVisualParentByType<T>(UIElement child) where T : UIElement
        {
            var parent = VisualTreeHelper.GetParent(child);
            if (parent != null && parent is UIElement)
            {
                if (parent is T)
                {
                    return parent as T;
                }
                return VisualTreeHelpers.GetVisualParentByType<T>(parent as UIElement);
            }
            return null;
        }
    }
}
