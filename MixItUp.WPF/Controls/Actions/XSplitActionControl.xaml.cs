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
    /// Interaction logic for XSplitActionControl.xaml
    /// </summary>
    public partial class XSplitActionControl : ActionControlBase
    {
        private enum XSplitTypeEnum
        {
            Scene,
            [Name("Source Visibility")]
            SourceVisibility,
            [Name("Text Source")]
            TextSource,
            [Name("Web Browser Source")]
            WebBrowserSource
        }

        private XSplitAction action;

        public XSplitActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public XSplitActionControl(ActionContainerControl containerControl, XSplitAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            this.XSplitTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<XSplitTypeEnum>();
            this.XSplitSourceVisibleCheckBox.IsChecked = true;
            if (this.action != null)
            {
                if (!string.IsNullOrEmpty(this.action.SceneName))
                {
                    this.XSplitTypeComboBox.SelectedItem = EnumHelper.GetEnumName(XSplitTypeEnum.Scene);
                    this.XSplitSceneNameTextBox.Text = this.action.SceneName;
                }
                else
                {
                    this.XSplitSourceNameTextBox.Text = this.action.SourceName;
                    this.XSplitSourceVisibleCheckBox.IsChecked = this.action.SourceVisible;
                    if (!string.IsNullOrEmpty(this.action.SourceText))
                    {
                        this.XSplitSourceTextTextBox.Text = this.action.SourceText;
                        this.XSplitSourceLoadTextFromTextBox.Text = this.action.LoadTextFromFilePath;
                        this.XSplitTypeComboBox.SelectedItem = EnumHelper.GetEnumName(XSplitTypeEnum.TextSource);
                    }
                    else if (!string.IsNullOrEmpty(this.action.SourceURL))
                    {
                        this.XSplitSourceWebPageTextBox.Text = this.action.SourceURL;
                        this.XSplitTypeComboBox.SelectedItem = EnumHelper.GetEnumName(XSplitTypeEnum.WebBrowserSource);
                    }
                    else
                    {
                        this.XSplitTypeComboBox.SelectedItem = EnumHelper.GetEnumName(XSplitTypeEnum.SourceVisibility);
                    }
                }
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (this.XSplitTypeComboBox.SelectedIndex >= 0)
            {
                if (this.XSplitSceneGrid.Visibility == Visibility.Visible && !string.IsNullOrEmpty(this.XSplitSceneNameTextBox.Text))
                {
                    return new XSplitAction(this.XSplitSceneNameTextBox.Text);
                }
                else if (this.XSplitSourceGrid.Visibility == Visibility.Visible && !string.IsNullOrEmpty(this.XSplitSourceNameTextBox.Text))
                {
                    if (this.XSplitSourceTextGrid.Visibility == Visibility.Visible)
                    {
                        if (!string.IsNullOrEmpty(this.XSplitSourceTextTextBox.Text))
                        {
                            XSplitAction action = new XSplitAction(this.XSplitSourceNameTextBox.Text, this.XSplitSourceVisibleCheckBox.IsChecked.GetValueOrDefault(), this.XSplitSourceTextTextBox.Text);
                            action.UpdateReferenceTextFile();
                            return action;
                        }
                    }
                    else if (this.XSplitSourceWebBrowserGrid.Visibility == Visibility.Visible)
                    {
                        if (!string.IsNullOrEmpty(this.XSplitSourceWebPageTextBox.Text))
                        {
                            return new XSplitAction(this.XSplitSourceNameTextBox.Text, this.XSplitSourceVisibleCheckBox.IsChecked.GetValueOrDefault(), null, this.XSplitSourceWebPageTextBox.Text);
                        }
                    }
                    else
                    {
                        XSplitAction action = new XSplitAction(this.XSplitSourceNameTextBox.Text, this.XSplitSourceVisibleCheckBox.IsChecked.GetValueOrDefault());
                        action.UpdateReferenceTextFile();
                        return action;
                    }
                }
            }
            return null;
        }

        private void XSplitTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.XSplitSceneGrid.Visibility = Visibility.Collapsed;
            this.XSplitSourceGrid.Visibility = Visibility.Collapsed;
            this.XSplitSourceTextGrid.Visibility = Visibility.Collapsed;
            this.XSplitSourceWebBrowserGrid.Visibility = Visibility.Collapsed;
            if (this.XSplitTypeComboBox.SelectedIndex >= 0)
            {
                XSplitTypeEnum xsplitType = EnumHelper.GetEnumValueFromString<XSplitTypeEnum>((string)this.XSplitTypeComboBox.SelectedItem);
                if (xsplitType == XSplitTypeEnum.Scene)
                {
                    this.XSplitSceneGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    this.XSplitSourceGrid.Visibility = Visibility.Visible;
                    if (xsplitType == XSplitTypeEnum.TextSource)
                    {
                        this.XSplitSourceTextGrid.Visibility = Visibility.Visible;
                    }
                    else if (xsplitType == XSplitTypeEnum.WebBrowserSource)
                    {
                        this.XSplitSourceWebBrowserGrid.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void XSplitSourceTextTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.XSplitSourceNameTextBox.Text))
            {
                this.XSplitSourceLoadTextFromTextBox.Text = Path.Combine(ChannelSession.Services.FileService.GetApplicationDirectory(), XSplitAction.XSplitReferenceTextFilesDirectory, this.XSplitSourceNameTextBox.Text + ".txt");
            }
        }

        private void XSplitSourceWebPageBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog();
            if (!string.IsNullOrEmpty(filePath))
            {
                this.XSplitSourceWebPageTextBox.Text = filePath;
            }
        }
    }
}
