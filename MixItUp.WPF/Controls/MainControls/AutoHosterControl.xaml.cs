using MixItUp.Base.ViewModel.Controls.MainControls;
using System.Diagnostics;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for AutoHosterControl.xaml
    /// </summary>
    public partial class AutoHosterControl : MainControlBase
    {
        public AutoHosterControl()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Process.Start("MixItUp.AutoHoster.exe");
        }
    }
}
