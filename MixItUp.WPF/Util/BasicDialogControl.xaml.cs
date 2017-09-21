using System.Windows.Controls;

namespace MixItUp.WPF.Util
{
    /// <summary>
    /// Interaction logic for BasicDialogControl.xaml
    /// </summary>
    public partial class BasicDialogControl : UserControl
    {
        public BasicDialogControl(string message)
        {
            this.DataContext = message;

            InitializeComponent();
        }
    }
}
