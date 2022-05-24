using MixItUp.Base.Model.Commands;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Dialogs
{
    /// <summary>
    /// Interaction logic for EditTestCommandParametersDialogControl.xaml
    /// </summary>
    public partial class EditTestCommandParametersDialogControl : UserControl
    {
        private CommandParametersModel parameters;

        private readonly ObservableCollection<DataWrapper> values = new ObservableCollection<DataWrapper>();

        public EditTestCommandParametersDialogControl(CommandParametersModel parameters)
        {
            InitializeComponent();

            this.parameters = parameters;

            this.ArgumentsTextBox.Text = string.Join(" ", this.parameters.Arguments);

            foreach (var kvp in this.parameters.SpecialIdentifiers)
            {
                this.values.Add(new DataWrapper { SpecialIdentifier = kvp.Key, Value = kvp.Value });
            }

            this.SpecialIdentifiersList.ItemsSource = values;
        }

        public CommandParametersModel GetCommandParameters()
        {
            this.parameters.Arguments = CommandParametersModel.GenerateArguments(this.ArgumentsTextBox.Text);

            this.parameters.SpecialIdentifiers.Clear();
            foreach (var value in this.values)
            {
                this.parameters.SpecialIdentifiers.Add(value.SpecialIdentifier, value.Value);
            }

            return this.parameters;
        }

        private class DataWrapper
        {
            public string SpecialIdentifier { get; set; }
            public string Value { get; set; }
        }
    }
}
