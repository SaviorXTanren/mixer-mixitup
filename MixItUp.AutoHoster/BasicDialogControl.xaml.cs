using System.Windows.Controls;

namespace MixItUp.AutoHoster
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
