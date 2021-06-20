using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Dialogs
{
    /// <summary>
    /// Interaction logic for BasicTextEntryDialogControl.xaml
    /// </summary>
    public partial class BasicTextEntryDialogControl : UserControl
    {
        public BasicTextEntryDialogControl(string textFieldName, string defaultValue = null, string description = null)
        {
            this.DataContext = textFieldName;

            InitializeComponent();

            this.TextEntryTextBox.Text = defaultValue;

            if (!string.IsNullOrEmpty(description))
            {
                this.DescriptionTextBlock.Text = description;
                this.DescriptionTextBlock.Visibility = System.Windows.Visibility.Visible;
            }
        }

        public string TextEntry { get { return this.TextEntryTextBox.Text; } }
    }
}
