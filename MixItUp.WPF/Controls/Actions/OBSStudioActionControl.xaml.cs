using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Services;
using MixItUp.WPF.Util;
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
            WebBrowserSource,
            [Name("Source Dimensions")]
            SourceDimensions,
        }

        private OBSStudioAction action;

        public OBSStudioActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public OBSStudioActionControl(ActionContainerControl containerControl, OBSStudioAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            this.OBSStudioTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<OBSStudioTypeEnum>();
            this.OBSStudioSourceVisibleCheckBox.IsChecked = true;
            this.OBSStudioSourceDimensionsXScaleTextBox.Text = "1";
            this.OBSStudioSourceDimensionsYScaleTextBox.Text = "1";

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
                    else if (this.action.SourceDimensions != null)
                    {
                        this.OBSStudioTypeComboBox.SelectedItem = EnumHelper.GetEnumName(OBSStudioTypeEnum.SourceDimensions);
                        this.OBSStudioSourceDimensionsXPositionTextBox.Text = this.action.SourceDimensions.X.ToString();
                        this.OBSStudioSourceDimensionsYPositionTextBox.Text = this.action.SourceDimensions.Y.ToString();
                        this.OBSStudioSourceDimensionsRotationTextBox.Text = this.action.SourceDimensions.Rotation.ToString();
                        this.OBSStudioSourceDimensionsXScaleTextBox.Text = this.action.SourceDimensions.XScale.ToString();
                        this.OBSStudioSourceDimensionsYScaleTextBox.Text = this.action.SourceDimensions.YScale.ToString();
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
                    else if (this.OBSStudioSourceDimensionsGrid.Visibility == Visibility.Visible)
                    {
                        int x, y, rotation;
                        float xScale, yScale;
                        if (int.TryParse(this.OBSStudioSourceDimensionsXPositionTextBox.Text, out x) && int.TryParse(this.OBSStudioSourceDimensionsYPositionTextBox.Text, out y) &&
                            int.TryParse(this.OBSStudioSourceDimensionsRotationTextBox.Text, out rotation) && float.TryParse(this.OBSStudioSourceDimensionsXScaleTextBox.Text, out xScale) &&
                            float.TryParse(this.OBSStudioSourceDimensionsYScaleTextBox.Text, out yScale))
                        {
                            return new OBSStudioAction(this.OBSStudioSourceNameTextBox.Text, this.OBSStudioSourceVisibleCheckBox.IsChecked.GetValueOrDefault(),
                                new OBSSourceDimensions() { X = x, Y = y, Rotation = rotation, XScale = xScale, YScale = yScale });
                        }
                    }
                    else
                    {
                        return new OBSStudioAction(this.OBSStudioSourceNameTextBox.Text, this.OBSStudioSourceVisibleCheckBox.IsChecked.GetValueOrDefault());
                    }
                }
            }
            return null;
        }

        private void OBSStudioTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.OBSStudioSceneGrid.Visibility = Visibility.Collapsed;
            this.OBSStudioSourceGrid.Visibility = Visibility.Collapsed;
            this.OBSStudioSourceTextGrid.Visibility = Visibility.Collapsed;
            this.OBSStudioSourceWebBrowserGrid.Visibility = Visibility.Collapsed;
            this.OBSStudioSourceDimensionsGrid.Visibility = Visibility.Collapsed;
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
                    else if (obsStudioType == OBSStudioTypeEnum.SourceDimensions)
                    {
                        this.OBSStudioSourceDimensionsGrid.Visibility = Visibility.Visible;
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

        private async void GetSourcesDimensionsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.OBSStudioSourceNameTextBox.Text))
            {
                await this.containerControl.RunAsyncOperation(async () =>
                {
                    if (ChannelSession.Services.OBSWebsocket != null || await ChannelSession.Services.InitializeOBSWebsocket())
                    {
                        OBSSourceDimensions dimensions = ChannelSession.Services.OBSWebsocket.GetSourceDimensions(this.OBSStudioSourceNameTextBox.Text);
                        this.OBSStudioSourceDimensionsXPositionTextBox.Text = dimensions.X.ToString();
                        this.OBSStudioSourceDimensionsYPositionTextBox.Text = dimensions.Y.ToString();
                        this.OBSStudioSourceDimensionsRotationTextBox.Text = dimensions.Rotation.ToString();
                        this.OBSStudioSourceDimensionsXScaleTextBox.Text = dimensions.XScale.ToString();
                        this.OBSStudioSourceDimensionsYScaleTextBox.Text = dimensions.YScale.ToString();
                    }
                    else
                    {
                        await MessageBoxHelper.ShowMessageDialog("Could not connect to OBS Studio. Please try establishing connection with it in the Services area.");
                    }
                });
            }
        }
    }
}
