using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Dialogs
{
    /// <summary>
    /// Interaction logic for EditTestSpecialIdentifiersDialogControl.xaml
    /// </summary>
    public partial class EditTestSpecialIdentifiersDialogControl : UserControl
    {
        private readonly ObservableCollection<DataWrapper> values = new ObservableCollection<DataWrapper>();

        public EditTestSpecialIdentifiersDialogControl(Dictionary<string, string> specialIdentifiers)
        {
            foreach (var kvp in specialIdentifiers)
            {
                this.values.Add(new DataWrapper { SpecialIdentifier = kvp.Key, Value = kvp.Value });
            }

            InitializeComponent();

            this.SpecialIdentifiersList.ItemsSource = values;
        }

        public Dictionary<string, string> GetSpecialIdentifiers()
        {
            Dictionary<string, string> results = new Dictionary<string, string>();
            foreach(var value in this.values)
            {
                results.Add(value.SpecialIdentifier, value.Value);
            }

            return results;
        }

        private class DataWrapper
        {
            public string SpecialIdentifier { get; set; }
            public string Value { get; set; }
        }
    }
}
