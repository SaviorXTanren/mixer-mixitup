using System.Windows.Controls;

namespace MixItUp.AutoHoster
{
    /// <summary>
    /// Interaction logic for BasicTextEntryDialogControl.xaml
    /// </summary>
    public partial class BasicTextEntryDialogControl : UserControl
    {
        public BasicTextEntryDialogControl(string textFieldName, string defaultValue = null)
        {
            this.DataContext = textFieldName;

            InitializeComponent();

            this.TextEntryTextBox.Text = defaultValue;
        }

        public string TextEntry { get { return this.TextEntryTextBox.Text; } }
    }
}
