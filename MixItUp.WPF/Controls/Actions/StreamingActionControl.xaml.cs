using Mixer.Base.Util;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Actions
{
    public enum StreamingActionTypeEnum
    {
        Scene,
        [Name("Source Visibility")]
        SourceVisibility,
        [Name("Text Source")]
        TextSource,
        [Name("Web Browser Source")]
        WebBrowserSource,
        [Name("Source Dimensions")]
        SourceDimensions,
    }

    /// <summary>
    /// Interaction logic for StreamingActionControl.xaml
    /// </summary>
    public partial class StreamingActionControl : UserControl
    {
        private ObservableCollection<string> actionTypes = new ObservableCollection<string>();

        public StreamingActionControl()
        {
            InitializeComponent();
        }

        private void StreamingSoftwareComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void StreamingActionTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void SourceTextTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void SourceWebPageBrowseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
