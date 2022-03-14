using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls
{
    /// <summary>
    /// Interaction logic for IconButton.xaml
    /// </summary>
    public partial class IconButton : Button
    {
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(
                "Icon",
                typeof(PackIconKind),
                typeof(IconButton),
                new FrameworkPropertyMetadata(
                    PackIconKind.Delete,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    new PropertyChangedCallback(OnIconChanged)
                ));

        public IconButton()
        {
            InitializeComponent();
        }

        public PackIconKind Icon
        {
            get { return (PackIconKind)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IconButton iconButton = (IconButton)d;
            if (iconButton != null && iconButton.ButtonIcon != null)
            {
                iconButton.ButtonIcon.Kind = iconButton.Icon;
            }
        }
    }
}
