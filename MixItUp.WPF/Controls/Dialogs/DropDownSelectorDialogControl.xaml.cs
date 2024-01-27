using MixItUp.Base.ViewModels;
using System.Collections.Generic;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Dialogs
{
    /// <summary>
    /// Interaction logic for DropDownSelectorDialogControl.xaml
    /// </summary>
    public partial class DropDownSelectorDialogControl : UserControl
    {
        private class DropDownSelectorDialogControlViewModel : UIViewModelBase
        {
            public string Description { get; set; }

            public List<string> Options { get; set; } = new List<string>();

            public string SelectedOption
            {
                get { return this.selectedOption; }
                set
                {
                    this.selectedOption = value;
                    this.NotifyPropertyChanged();
                }
            }
            private string selectedOption;
        }

        public string SelectedOption { get { return this.viewModel.SelectedOption; } }

        private DropDownSelectorDialogControlViewModel viewModel;

        public DropDownSelectorDialogControl(IEnumerable<string> options, string description = null)
        {
            this.DataContext = this.viewModel = new DropDownSelectorDialogControlViewModel();

            this.viewModel.Options.AddRange(options);

            if (!string.IsNullOrEmpty(description))
            {
                this.viewModel.Description = description;
            }

            InitializeComponent();
        }
    }
}
