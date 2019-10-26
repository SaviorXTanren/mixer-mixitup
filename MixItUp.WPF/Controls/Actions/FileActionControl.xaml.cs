using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
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
        private FileAction action;

        public FileActionControl() : base() { InitializeComponent(); }

        public FileActionControl(FileAction action) : this() { this.action = action; }

        public override Task OnLoaded()
        {
            this.FileActionTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<FileActionTypeEnum>().OrderBy(s => s);
            if (this.action != null)
            {
                this.FileActionTypeComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.FileActionType);
                if (this.action.FileActionType == FileActionTypeEnum.SaveToFile || this.action.FileActionType == FileActionTypeEnum.AppendToFile)
                {
                    this.SaveToFileTextTextBox.Text = this.action.TransferText;
                }
                else if (this.action.FileActionType == FileActionTypeEnum.ReadFromFile || this.action.FileActionType == FileActionTypeEnum.ReadSpecificLineFromFile ||
                    this.action.FileActionType == FileActionTypeEnum.ReadRandomLineFromFile || this.action.FileActionType == FileActionTypeEnum.RemoveSpecificLineFromFile ||
                    this.action.FileActionType == FileActionTypeEnum.RemoveRandomLineFromFile)
                {
                    this.SpecialIdentifierNameTextBox.Text = this.action.TransferText;
                    if (this.action.FileActionType == FileActionTypeEnum.ReadSpecificLineFromFile || this.action.FileActionType == FileActionTypeEnum.RemoveSpecificLineFromFile)
                    {
                        this.SpecificLineTextBox.Text = this.action.LineIndexToRead;
                    }
                }
                this.FilePathTextBox.Text = this.action.FilePath;
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (!string.IsNullOrEmpty(this.FilePathTextBox.Text))
            {
                FileActionTypeEnum fileType = EnumHelper.GetEnumValueFromString<FileActionTypeEnum>((string)this.FileActionTypeComboBox.SelectedItem);
                if (fileType == FileActionTypeEnum.SaveToFile || fileType == FileActionTypeEnum.AppendToFile)
                {
                    string transferText = this.SaveToFileTextTextBox.Text;
                    if (string.IsNullOrEmpty(transferText))
                    {
                        transferText = string.Empty;
                    }
                    return new FileAction(fileType, transferText: this.SaveToFileTextTextBox.Text, filePath: this.FilePathTextBox.Text);
                }
                else if (fileType == FileActionTypeEnum.ReadFromFile || fileType == FileActionTypeEnum.ReadSpecificLineFromFile || fileType == FileActionTypeEnum.ReadRandomLineFromFile ||
                    fileType == FileActionTypeEnum.RemoveSpecificLineFromFile || fileType == FileActionTypeEnum.RemoveRandomLineFromFile)
                {
                    if (SpecialIdentifierStringBuilder.IsValidSpecialIdentifier(this.SpecialIdentifierNameTextBox.Text))
                    {
                        FileAction action = new FileAction(fileType, transferText: this.SpecialIdentifierNameTextBox.Text, filePath: this.FilePathTextBox.Text);
                        if (fileType == FileActionTypeEnum.ReadSpecificLineFromFile || fileType == FileActionTypeEnum.RemoveSpecificLineFromFile)
                        {
                            if (string.IsNullOrEmpty(this.SpecificLineTextBox.Text))
                            {
                                return null;
                            }
                            action.LineIndexToRead = this.SpecificLineTextBox.Text;
                        }
                        return action;
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
                this.SpecificLineTextBox.Visibility = Visibility.Collapsed;

                FileActionTypeEnum fileType = EnumHelper.GetEnumValueFromString<FileActionTypeEnum>((string)this.FileActionTypeComboBox.SelectedItem);
                if (fileType == FileActionTypeEnum.SaveToFile || fileType == FileActionTypeEnum.AppendToFile)
                {
                    this.SaveToFileGrid.Visibility = Visibility.Visible;
                }
                else if (fileType == FileActionTypeEnum.ReadFromFile || fileType == FileActionTypeEnum.ReadSpecificLineFromFile || fileType == FileActionTypeEnum.ReadRandomLineFromFile ||
                    fileType == FileActionTypeEnum.RemoveSpecificLineFromFile || fileType == FileActionTypeEnum.RemoveRandomLineFromFile)
                {
                    this.ReadFromFileGrid.Visibility = Visibility.Visible;
                    if (fileType == FileActionTypeEnum.ReadSpecificLineFromFile || fileType == FileActionTypeEnum.RemoveSpecificLineFromFile)
                    {
                        this.SpecificLineTextBox.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void FileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            FileActionTypeEnum fileType = EnumHelper.GetEnumValueFromString<FileActionTypeEnum>((string)this.FileActionTypeComboBox.SelectedItem);
            string filePath = (fileType == FileActionTypeEnum.SaveToFile || fileType == FileActionTypeEnum.AppendToFile) ?
                ChannelSession.Services.FileService.ShowSaveFileDialog("") : ChannelSession.Services.FileService.ShowOpenFileDialog();
            if (!string.IsNullOrEmpty(filePath))
            {
                this.FilePathTextBox.Text = filePath;
            }
        }
    }
}
