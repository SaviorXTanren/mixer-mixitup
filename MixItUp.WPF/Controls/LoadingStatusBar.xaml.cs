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

        public void SetStatusText(string text) { this.StatusTextBlock.Text = text; }

        public void ShowProgressBar() { this.StatusProgressBar.Visibility = Visibility.Visible; }

        public void HideProgressBar() { this.StatusProgressBar.Visibility = Visibility.Collapsed; }
    }
}
