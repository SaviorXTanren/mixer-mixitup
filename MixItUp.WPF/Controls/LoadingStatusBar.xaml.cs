using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls
{
    /// <summary>
    /// Interaction logic for LoadingStatusBar.xaml
    /// </summary>
    public partial class LoadingStatusBar : UserControl
    {
        public LoadingStatusBar()
        {
            InitializeComponent();
        }

        public void ShowProgressBar() { this.StatusBar.Visibility = Visibility.Visible; }

        public void HideProgressBar() { this.StatusBar.Visibility = Visibility.Hidden; }
    }
}
