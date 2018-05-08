using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for StreamlabsOBSActionControl.xaml
    /// </summary>
    public partial class StreamlabsOBSActionControl : ActionControlBase
    {
        private enum StreamlabsOBSActionTypeEnum
        {
            Scene,
            [Name("Source Visibility")]
            SourceVisibility,
            [Name("Text Source")]
            TextSource,
        }

        private StreamlabsOBSAction action;

        public StreamlabsOBSActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public StreamlabsOBSActionControl(ActionContainerControl containerControl, StreamlabsOBSAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            this.StreamlabsOBSNotEnabledWarningTextBlock.Visibility = (ChannelSession.Services.StreamlabsOBSService == null) ? Visibility.Visible : Visibility.Collapsed;

            this.StreamlabsOBSTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<StreamlabsOBSActionTypeEnum>();
            this.StreamlabsOBSSourceVisibleCheckBox.IsChecked = true;

            if (this.action != null)
            {
                if (!string.IsNullOrEmpty(this.action.SceneName))
                {
                    this.StreamlabsOBSTypeComboBox.SelectedItem = EnumHelper.GetEnumName(StreamlabsOBSActionTypeEnum.Scene);
                    this.StreamlabsOBSSceneNameTextBox.Text = this.action.SceneName;
                }
                else
                {
                    this.StreamlabsOBSSourceNameTextBox.Text = this.action.SourceName;
                    this.StreamlabsOBSSourceVisibleCheckBox.IsChecked = this.action.SourceVisible;
                    if (!string.IsNullOrEmpty(this.action.SourceText))
                    {
                        this.StreamlabsOBSSourceTextTextBox.Text = this.action.SourceText;
                        this.StreamlabsOBSSourceLoadTextFromTextBox.Text = this.action.LoadTextFromFilePath;
                        this.StreamlabsOBSTypeComboBox.SelectedItem = EnumHelper.GetEnumName(StreamlabsOBSActionTypeEnum.TextSource);
                    }
                    else
                    {
                        this.StreamlabsOBSTypeComboBox.SelectedItem = EnumHelper.GetEnumName(StreamlabsOBSActionTypeEnum.SourceVisibility);
                    }
                }
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (this.StreamlabsOBSTypeComboBox.SelectedIndex >= 0)
            {
                if (this.StreamlabsOBSSceneGrid.Visibility == Visibility.Visible && !string.IsNullOrEmpty(this.StreamlabsOBSSceneNameTextBox.Text))
                {
                    return new StreamlabsOBSAction(this.StreamlabsOBSSceneNameTextBox.Text);
                }
                else if (this.StreamlabsOBSSourceGrid.Visibility == Visibility.Visible && !string.IsNullOrEmpty(this.StreamlabsOBSSourceNameTextBox.Text))
                {
                    if (this.StreamlabsOBSSourceTextGrid.Visibility == Visibility.Visible)
                    {
                        if (!string.IsNullOrEmpty(this.StreamlabsOBSSourceTextTextBox.Text))
                        {
                            StreamlabsOBSAction action = new StreamlabsOBSAction(this.StreamlabsOBSSourceNameTextBox.Text, this.StreamlabsOBSSourceVisibleCheckBox.IsChecked.GetValueOrDefault(),
                                this.StreamlabsOBSSourceTextTextBox.Text);
                            action.UpdateReferenceTextFile();
                            return action;
                        }
                    }
                    else
                    {
                        return new StreamlabsOBSAction(this.StreamlabsOBSSourceNameTextBox.Text, this.StreamlabsOBSSourceVisibleCheckBox.IsChecked.GetValueOrDefault());
                    }
                }
            }
            return null;
        }

        private void StreamlabsOBSTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.StreamlabsOBSSceneGrid.Visibility = Visibility.Collapsed;
            this.StreamlabsOBSSourceGrid.Visibility = Visibility.Collapsed;
            this.StreamlabsOBSSourceTextGrid.Visibility = Visibility.Collapsed;
            if (this.StreamlabsOBSTypeComboBox.SelectedIndex >= 0)
            {
                StreamlabsOBSActionTypeEnum obsStudioType = EnumHelper.GetEnumValueFromString<StreamlabsOBSActionTypeEnum>((string)this.StreamlabsOBSTypeComboBox.SelectedItem);
                if (obsStudioType == StreamlabsOBSActionTypeEnum.Scene)
                {
                    this.StreamlabsOBSSceneGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    this.StreamlabsOBSSourceGrid.Visibility = Visibility.Visible;
                    if (obsStudioType == StreamlabsOBSActionTypeEnum.TextSource)
                    {
                        this.StreamlabsOBSSourceTextGrid.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void StreamlabsOBSSourceTextTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.StreamlabsOBSSourceNameTextBox.Text))
            {
                this.StreamlabsOBSSourceLoadTextFromTextBox.Text = Path.Combine(ChannelSession.Services.FileService.GetApplicationDirectory(), StreamlabsOBSAction.StreamlabsOBSStudioReferenceTextFilesDirectory, this.StreamlabsOBSSourceNameTextBox.Text + ".txt");
            }
        }
    }
}
