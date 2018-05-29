using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.WPF.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
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
    public partial class StreamingActionControl : ActionControlBase
    {
        private StreamingAction action;
        private ObservableCollection<string> actionTypes = new ObservableCollection<string>();

        public StreamingActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public StreamingActionControl(ActionContainerControl containerControl, StreamingAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            this.StreamingSoftwareComboBox.ItemsSource = EnumHelper.GetEnumNames<StreamingSoftwareTypeEnum>();

            this.StreamingSoftwareComboBox.SelectedItem = EnumHelper.GetEnumName(StreamingSoftwareTypeEnum.DefaultSetting);
            this.SourceVisibleCheckBox.IsChecked = true;
            this.SourceDimensionsXScaleTextBox.Text = "1";
            this.SourceDimensionsYScaleTextBox.Text = "1";

            if (this.action != null)
            {
                this.StreamingSoftwareComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.SoftwareType);

                if (!string.IsNullOrEmpty(this.action.SceneName))
                {
                    this.StreamingActionTypeComboBox.SelectedItem = EnumHelper.GetEnumName(StreamingActionTypeEnum.Scene);
                    this.SceneNameTextBox.Text = this.action.SceneName;
                }
                else
                {
                    this.SourceNameTextBox.Text = this.action.SourceName;
                    this.SourceVisibleCheckBox.IsChecked = this.action.SourceVisible;
                    if (!string.IsNullOrEmpty(this.action.SourceText))
                    {
                        this.SourceTextTextBox.Text = this.action.SourceText;
                        this.SourceLoadTextFromTextBox.Text = this.action.SourceTextFilePath;
                        this.StreamingActionTypeComboBox.SelectedItem = EnumHelper.GetEnumName(StreamingActionTypeEnum.TextSource);
                    }
                    else if (!string.IsNullOrEmpty(this.action.SourceURL))
                    {
                        this.SourceWebPageTextBox.Text = this.action.SourceURL;
                        this.StreamingActionTypeComboBox.SelectedItem = EnumHelper.GetEnumName(StreamingActionTypeEnum.WebBrowserSource);
                    }
                    else if (this.action.SourceDimensions != null)
                    {
                        this.StreamingActionTypeComboBox.SelectedItem = EnumHelper.GetEnumName(StreamingActionTypeEnum.SourceDimensions);
                        this.SourceDimensionsXPositionTextBox.Text = this.action.SourceDimensions.X.ToString();
                        this.SourceDimensionsYPositionTextBox.Text = this.action.SourceDimensions.Y.ToString();
                        this.SourceDimensionsRotationTextBox.Text = this.action.SourceDimensions.Rotation.ToString();
                        this.SourceDimensionsXScaleTextBox.Text = this.action.SourceDimensions.XScale.ToString();
                        this.SourceDimensionsYScaleTextBox.Text = this.action.SourceDimensions.YScale.ToString();
                    }
                    else
                    {
                        this.StreamingActionTypeComboBox.SelectedItem = EnumHelper.GetEnumName(StreamingActionTypeEnum.SourceVisibility);
                    }
                }
            }

            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (this.StreamingSoftwareComboBox.SelectedIndex >= 0 && this.StreamingActionTypeComboBox.SelectedIndex >= 0)
            {
                StreamingSoftwareTypeEnum software = EnumHelper.GetEnumValueFromString<StreamingSoftwareTypeEnum>((string)this.StreamingSoftwareComboBox.SelectedItem);
                StreamingActionTypeEnum type = EnumHelper.GetEnumValueFromString<StreamingActionTypeEnum>((string)this.StreamingActionTypeComboBox.SelectedItem);

                if (type == StreamingActionTypeEnum.Scene && !string.IsNullOrEmpty(this.SceneNameTextBox.Text))
                {
                    return StreamingAction.CreateSceneAction(software, this.SceneNameTextBox.Text);
                }
                else if (!string.IsNullOrEmpty(this.SourceNameTextBox.Text))
                {
                    if (type == StreamingActionTypeEnum.TextSource)
                    {
                        if (!string.IsNullOrEmpty(this.SourceTextTextBox.Text) && !string.IsNullOrEmpty(this.SourceLoadTextFromTextBox.Text))
                        {
                            StreamingAction action = StreamingAction.CreateSourceTextAction(software, this.SourceNameTextBox.Text, this.SourceVisibleCheckBox.IsChecked.GetValueOrDefault(), this.SourceTextTextBox.Text, this.SourceLoadTextFromTextBox.Text);
                            action.UpdateReferenceTextFile(string.Empty);
                            return action;
                        }
                    }
                    else if (type == StreamingActionTypeEnum.WebBrowserSource)
                    {
                        if (!string.IsNullOrEmpty(this.SourceWebPageTextBox.Text))
                        {
                            return StreamingAction.CreateSourceURLAction(software, this.SourceNameTextBox.Text, this.SourceVisibleCheckBox.IsChecked.GetValueOrDefault(), this.SourceWebPageTextBox.Text);
                        }
                    }
                    else if (type == StreamingActionTypeEnum.SourceDimensions)
                    {
                        int x, y, rotation;
                        float xScale, yScale;
                        if (int.TryParse(this.SourceDimensionsXPositionTextBox.Text, out x) && int.TryParse(this.SourceDimensionsYPositionTextBox.Text, out y) &&
                            int.TryParse(this.SourceDimensionsRotationTextBox.Text, out rotation) && float.TryParse(this.SourceDimensionsXScaleTextBox.Text, out xScale) &&
                            float.TryParse(this.SourceDimensionsYScaleTextBox.Text, out yScale))
                        {
                            return StreamingAction.CreateSourceDimensionsAction(software, this.SourceNameTextBox.Text, this.SourceVisibleCheckBox.IsChecked.GetValueOrDefault(),
                                new StreamingSourceDimensions() { X = x, Y = y, Rotation = rotation, XScale = xScale, YScale = yScale });
                        }
                    }
                    else
                    {
                        return StreamingAction.CreateSourceVisibilityAction(software, this.SourceNameTextBox.Text, this.SourceVisibleCheckBox.IsChecked.GetValueOrDefault());
                    }
                }
            }
            return null;
        }

        private void StreamingSoftwareComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.OBSStudioNotEnabledWarningTextBlock.Visibility = Visibility.Collapsed;
            this.XSplitNotEnabledWarningTextBlock.Visibility = Visibility.Collapsed;
            this.StreamlabsOBSNotEnabledWarningTextBlock.Visibility = Visibility.Collapsed;

            if (this.StreamingSoftwareComboBox.SelectedIndex >= 0)
            {
                StreamingSoftwareTypeEnum software = this.GetSelectedSoftware();
                if (software == StreamingSoftwareTypeEnum.OBSStudio)
                {
                    this.OBSStudioNotEnabledWarningTextBlock.Visibility = (ChannelSession.Services.OBSWebsocket == null) ? Visibility.Visible : Visibility.Collapsed;
                    this.StreamingActionTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<StreamingActionTypeEnum>();
                }
                else if (software == StreamingSoftwareTypeEnum.XSplit)
                {
                    this.XSplitNotEnabledWarningTextBlock.Visibility = (ChannelSession.Services.XSplitServer == null) ? Visibility.Visible : Visibility.Collapsed;
                    this.StreamingActionTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<StreamingActionTypeEnum>(new List<StreamingActionTypeEnum>() { StreamingActionTypeEnum.Scene, StreamingActionTypeEnum.SourceVisibility, StreamingActionTypeEnum.TextSource, StreamingActionTypeEnum.WebBrowserSource });
                }
                else if (software == StreamingSoftwareTypeEnum.StreamlabsOBS)
                {
                    this.StreamlabsOBSNotEnabledWarningTextBlock.Visibility = (ChannelSession.Services.StreamlabsOBSService == null) ? Visibility.Visible : Visibility.Collapsed;
                    this.StreamingActionTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<StreamingActionTypeEnum>(new List<StreamingActionTypeEnum>() { StreamingActionTypeEnum.Scene, StreamingActionTypeEnum.SourceVisibility, StreamingActionTypeEnum.TextSource });
                }
            }
        }

        private void StreamingActionTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.StreamingActionTypeComboBox.SelectedIndex >= 0)
            {
                StreamingActionTypeEnum type = EnumHelper.GetEnumValueFromString<StreamingActionTypeEnum>((string)this.StreamingActionTypeComboBox.SelectedItem);
                if (type == StreamingActionTypeEnum.Scene)
                {
                    this.SceneGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    this.SourceGrid.Visibility = Visibility.Visible;
                    if (type == StreamingActionTypeEnum.TextSource)
                    {
                        this.SourceTextGrid.Visibility = Visibility.Visible;
                    }
                    else if (type == StreamingActionTypeEnum.WebBrowserSource)
                    {
                        this.SourceWebBrowserGrid.Visibility = Visibility.Visible;
                    }
                    else if (type == StreamingActionTypeEnum.SourceDimensions)
                    {
                        this.SourceDimensionsGrid.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void SourceLoadTextFromBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            string name = (!string.IsNullOrEmpty(this.SourceNameTextBox.Text)) ? this.SourceNameTextBox.Text + ".txt" : "Source.txt";
            if (this.action != null && !string.IsNullOrEmpty(this.action.SourceTextFilePath))
            {
                name = this.action.SourceTextFilePath;
            }
            string filePath = ChannelSession.Services.FileService.ShowSaveFileDialog(name);
            if (!string.IsNullOrEmpty(filePath))
            {
                this.SourceLoadTextFromTextBox.Text = filePath;
            }
        }

        private void SourceWebPageBrowseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog();
            if (!string.IsNullOrEmpty(filePath))
            {
                this.SourceWebPageTextBox.Text = filePath;
            }
        }

        private async void GetSourcesDimensionsButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.StreamingSoftwareComboBox.SelectedIndex >= 0 && !string.IsNullOrEmpty(this.SourceNameTextBox.Text))
            {
                StreamingSoftwareTypeEnum software = this.GetSelectedSoftware();
                if (software == StreamingSoftwareTypeEnum.OBSStudio)
                {
                    await this.containerControl.RunAsyncOperation(async () =>
                    {
                        if (ChannelSession.Services.OBSWebsocket != null || await ChannelSession.Services.InitializeOBSWebsocket())
                        {
                            StreamingSourceDimensions dimensions = ChannelSession.Services.OBSWebsocket.GetSourceDimensions(this.SourceNameTextBox.Text);
                            this.SourceDimensionsXPositionTextBox.Text = dimensions.X.ToString();
                            this.SourceDimensionsYPositionTextBox.Text = dimensions.Y.ToString();
                            this.SourceDimensionsRotationTextBox.Text = dimensions.Rotation.ToString();
                            this.SourceDimensionsXScaleTextBox.Text = dimensions.XScale.ToString();
                            this.SourceDimensionsYScaleTextBox.Text = dimensions.YScale.ToString();
                        }
                        else
                        {
                            await MessageBoxHelper.ShowMessageDialog("Could not connect to OBS Studio. Please try establishing connection with it in the Services area.");
                        }
                    });
                }
            }
        }

        private StreamingSoftwareTypeEnum GetSelectedSoftware()
        {
            StreamingSoftwareTypeEnum software = EnumHelper.GetEnumValueFromString<StreamingSoftwareTypeEnum>((string)this.StreamingSoftwareComboBox.SelectedItem);
            if (software == StreamingSoftwareTypeEnum.DefaultSetting)
            {
                software = ChannelSession.Settings.DefaultStreamingSoftware;
            }
            return software;
        }
    }
}
