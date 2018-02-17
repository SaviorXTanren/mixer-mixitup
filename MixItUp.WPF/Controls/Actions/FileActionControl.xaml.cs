using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for FileActionControl.xaml
    /// </summary>
    public partial class FileActionControl : ActionControlBase
    {
        private enum FileTypeEnum
        {
            [Name("Save To File")]
            SaveToFile,
            [Name("Read From File")]
            ReadFromFile,
        }

        private FileAction action;

        public FileActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public FileActionControl(ActionContainerControl containerControl, FileAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            this.FileActionTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<FileTypeEnum>();
            if (this.action != null)
            {
                this.FilePathTextBox.Text = this.action.FilePath;
                if (this.action.SaveToFile)
                {
                    this.FileActionTypeComboBox.SelectedItem = EnumHelper.GetEnumName(FileTypeEnum.SaveToFile);
                    this.SaveToFileTextTextBox.Text = this.action.TransferText;
                }
                else
                {
                    this.FileActionTypeComboBox.SelectedItem = EnumHelper.GetEnumName(FileTypeEnum.ReadFromFile);
                    this.SpecialIdentifierNameTextBox.Text = this.action.TransferText;
                }
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (!string.IsNullOrEmpty(this.FilePathTextBox.Text))
            {
                FileTypeEnum fileType = EnumHelper.GetEnumValueFromString<FileTypeEnum>((string)this.FileActionTypeComboBox.SelectedItem);
                if (fileType == FileTypeEnum.SaveToFile)
                {
                    if (!string.IsNullOrEmpty(this.SaveToFileTextTextBox.Text))
                    {
                        return new FileAction(saveToFile: true, transferText: this.SaveToFileTextTextBox.Text, filePath: this.FilePathTextBox.Text);
                    }
                }
                else if (fileType == FileTypeEnum.ReadFromFile)
                {
                    if (!string.IsNullOrEmpty(this.SpecialIdentifierNameTextBox.Text) && this.SpecialIdentifierNameTextBox.Text.All(c => Char.IsLetterOrDigit(c)))
                    {
                        return new FileAction(saveToFile: false, transferText: this.SpecialIdentifierNameTextBox.Text, filePath: this.FilePathTextBox.Text);
                    }
                }
            }
            return null;
        }

        private void FileActionTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.FileActionTypeComboBox.SelectedIndex >= 0)
            {
                this.FileGrid.Visibility = Visibility.Visible;
                this.SaveToFileGrid.Visibility = Visibility.Collapsed;
                this.ReadFromFileGrid.Visibility = Visibility.Collapsed;

                FileTypeEnum fileType = EnumHelper.GetEnumValueFromString<FileTypeEnum>((string)this.FileActionTypeComboBox.SelectedItem);
                if (fileType == FileTypeEnum.SaveToFile)
                {
                    this.SaveToFileGrid.Visibility = Visibility.Visible;
                }
                else if (fileType == FileTypeEnum.ReadFromFile)
                {
                    this.ReadFromFileGrid.Visibility = Visibility.Visible;
                }
            }
        }

        private void FileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            FileTypeEnum fileType = EnumHelper.GetEnumValueFromString<FileTypeEnum>((string)this.FileActionTypeComboBox.SelectedItem);
            string filePath = (fileType == FileTypeEnum.SaveToFile) ? ChannelSession.Services.FileService.ShowSaveFileDialog("") : ChannelSession.Services.FileService.ShowOpenFileDialog();
            if (!string.IsNullOrEmpty(filePath))
            {
                this.FilePathTextBox.Text = filePath;
            }
        }
    }
}
