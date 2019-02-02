using System.Windows;

namespace MixItUp.WPF.Windows
{
    /// <summary>
    /// Interaction logic for LicenseAgreementWindow.xaml
    /// </summary>
    public partial class LicenseAgreementWindow : Window
    {
        public bool Accepted = false;

        public LicenseAgreementWindow()
        {
            InitializeComponent();
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            this.Accepted = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
