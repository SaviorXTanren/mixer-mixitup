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
    /// Interaction logic for OBSStudioActionControl.xaml
    /// </summary>
    public partial class OBSStudioActionControl : ActionControlBase
    {
        private enum OBSStudioTypeEnum
        {
            Scene,
            [Name("Source Visibility")]
            SourceVisibility,
            [Name("Text Source")]
            TextSource,
            [Name("Web Browser Source")]
            WebBrowserSource
        }

        private OBSStudioAction action;

        public OBSStudioActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public OBSStudioActionControl(ActionContainerControl containerControl, OBSStudioAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            this.OBSStudioTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<OBSStudioTypeEnum>();
            if (this.action != null)
            {
                if (!string.IsNullOrEmpty(this.action.SceneName))
                {
                    this.OBSStudioTypeComboBox.SelectedItem = EnumHelper.GetEnumName(OBSStudioTypeEnum.Scene);
                    this.OBSStudioSceneCollectionNameTextBox.Text = this.action.SceneCollection;
                    this.OBSStudioSceneNameTextBox.Text = this.action.SceneName;
                }
                else
                {
                    this.OBSStudioSourceNameTextBox.Text = this.action.SourceName;
                    this.OBSStudioSourceVisibleCheckBox.IsChecked = this.action.SourceVisible;
                    if (!string.IsNullOrEmpty(this.action.SourceText))
                    {
                        this.OBSStudioSourceTextTextBox.Text = this.action.SourceText;
                        this.OBSStudioSourceLoadTextFromTextBox.Text = this.action.LoadTextFromFilePath;
                        this.OBSStudioTypeComboBox.SelectedItem = EnumHelper.GetEnumName(OBSStudioTypeEnum.TextSource);
                    }
                    else if (!string.IsNullOrEmpty(this.action.SourceURL))
                    {
                        this.OBSStudioSourceWebPageTextBox.Text = this.action.SourceURL;
                        this.OBSStudioTypeComboBox.SelectedItem = EnumHelper.GetEnumName(OBSStudioTypeEnum.WebBrowserSource);
                    }
                    else
                    {
                        this.OBSStudioTypeComboBox.SelectedItem = EnumHelper.GetEnumName(OBSStudioTypeEnum.SourceVisibility);
                    }
                }
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (this.OBSStudioTypeComboBox.SelectedIndex >= 0)
            {
                if (this.OBSStudioSceneGrid.Visibility == Visibility.Visible && !string.IsNullOrEmpty(this.OBSStudioSceneNameTextBox.Text))
                {
                    return new OBSStudioAction(this.OBSStudioSceneCollectionNameTextBox.Text, this.OBSStudioSceneNameTextBox.Text);
                }
                else if (this.OBSStudioSourceGrid.Visibility == Visibility.Visible && !string.IsNullOrEmpty(this.OBSStudioSourceNameTextBox.Text))
                {
                    if (this.OBSStudioSourceTextGrid.Visibility == Visibility.Visible)
                    {
                        if (!string.IsNullOrEmpty(this.OBSStudioSourceTextTextBox.Text))
                        {
                            OBSStudioAction action = new OBSStudioAction(this.OBSStudioSourceNameTextBox.Text, this.OBSStudioSourceVisibleCheckBox.IsChecked.GetValueOrDefault(), this.OBSStudioSourceTextTextBox.Text);
                            action.UpdateReferenceTextFile();
                            return action;
                        }
                    }
                    else if (this.OBSStudioSourceWebBrowserGrid.Visibility == Visibility.Visible)
                    {
                        if (!string.IsNullOrEmpty(this.OBSStudioSourceWebPageTextBox.Text))
                        {
                            return new OBSStudioAction(this.OBSStudioSourceNameTextBox.Text, this.OBSStudioSourceVisibleCheckBox.IsChecked.GetValueOrDefault(), null, this.OBSStudioSourceWebPageTextBox.Text);
                        }
                    }
                    else
                    {
                        OBSStudioAction action = new OBSStudioAction(this.OBSStudioSourceNameTextBox.Text, this.OBSStudioSourceVisibleCheckBox.IsChecked.GetValueOrDefault());
                        return action;
                    }
                }
            }
            return null;
        }

        private void OBSStudioTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.OBSStudioSceneGrid.Visibility = Visibility.Hidden;
            this.OBSStudioSourceGrid.Visibility = Visibility.Hidden;
            this.OBSStudioSourceTextGrid.Visibility = Visibility.Hidden;
            this.OBSStudioSourceWebBrowserGrid.Visibility = Visibility.Hidden;
            if (this.OBSStudioTypeComboBox.SelectedIndex >= 0)
            {
                OBSStudioTypeEnum obsStudioType = EnumHelper.GetEnumValueFromString<OBSStudioTypeEnum>((string)this.OBSStudioTypeComboBox.SelectedItem);
                if (obsStudioType == OBSStudioTypeEnum.Scene)
                {
                    this.OBSStudioSceneGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    this.OBSStudioSourceGrid.Visibility = Visibility.Visible;
                    if (obsStudioType == OBSStudioTypeEnum.TextSource)
                    {
                        this.OBSStudioSourceTextGrid.Visibility = Visibility.Visible;
                    }
                    else if (obsStudioType == OBSStudioTypeEnum.WebBrowserSource)
                    {
                        this.OBSStudioSourceWebBrowserGrid.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void OBSStudioSourceTextTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.OBSStudioSourceNameTextBox.Text))
            {
                this.OBSStudioSourceLoadTextFromTextBox.Text = Path.Combine(OBSStudioAction.OBSStudioReferenceTextFilesDirectory, this.OBSStudioSourceNameTextBox.Text + ".txt");
            }
        }

        private void OBSStudioSourceWebPageBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog();
            if (!string.IsNullOrEmpty(filePath))
            {
                this.OBSStudioSourceWebPageTextBox.Text = filePath;
            }
        }
    }
}
