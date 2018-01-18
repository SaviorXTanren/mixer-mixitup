using MixItUp.Base;
using MixItUp.Base.Actions;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for ExternalProgramActionControl.xaml
    /// </summary>
    public partial class ExternalProgramActionControl : ActionControlBase
    {
        private ExternalProgramAction action;

        public ExternalProgramActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public ExternalProgramActionControl(ActionContainerControl containerControl, ExternalProgramAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            if (this.action != null)
            {
                ExternalProgramAction externalAction = (ExternalProgramAction)this.action;
                this.ProgramFilePathTextBox.Text = externalAction.FilePath;
                this.ProgramArgumentsTextBox.Text = externalAction.Arguments;
                this.ShowWindowCheckBox.IsChecked = externalAction.ShowWindow;
                this.WaitForFinishCheckBox.IsChecked = externalAction.WaitForFinish;
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (!string.IsNullOrEmpty(this.ProgramFilePathTextBox.Text))
            {
                return new ExternalProgramAction(this.ProgramFilePathTextBox.Text, this.ProgramArgumentsTextBox.Text, this.ShowWindowCheckBox.IsChecked.GetValueOrDefault(),
                    this.WaitForFinishCheckBox.IsChecked.GetValueOrDefault());
            }
            return null;
        }

        private void ProgramFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog();
            if (!string.IsNullOrEmpty(filePath))
            {
                this.ProgramFilePathTextBox.Text = filePath;
            }
        }
    }
}
