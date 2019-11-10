using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Util;
using MixItUp.WPF.Util;
using StreamingClient.Base.Util;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for StreamingSoftwareActionControl.xaml
    /// </summary>
    public partial class StreamingSoftwareActionControl : ActionControlBase
    {
        private StreamingSoftwareAction action;

        public StreamingSoftwareActionControl() : base() { InitializeComponent(); }

        public StreamingSoftwareActionControl(StreamingSoftwareAction action) : this() { this.action = action; }

        public override Task OnLoaded()
        {
            this.StreamingSoftwareComboBox.ItemsSource = EnumHelper.GetEnumNames<StreamingSoftwareTypeEnum>();
            this.StreamingActionTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<StreamingActionTypeEnum>();

            this.StreamingSoftwareComboBox.SelectedItem = EnumHelper.GetEnumName(StreamingSoftwareTypeEnum.DefaultSetting);
            this.SourceVisibleCheckBox.IsChecked = true;
            this.SourceDimensionsXScaleTextBox.Text = "1";
            this.SourceDimensionsYScaleTextBox.Text = "1";

            if (this.action != null)
            {
                this.StreamingSoftwareComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.SoftwareType);
                this.StreamingActionTypeComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.ActionType);

                if (this.action.ActionType == StreamingActionTypeEnum.Scene)
                {
                    this.SceneNameTextBox.Text = this.action.SceneName;
                }
                else if (this.action.ActionType == StreamingActionTypeEnum.StartStopStream)
                {
                    // Do nothing...
                }
                else if (this.action.ActionType == StreamingActionTypeEnum.SaveReplayBuffer)
                {
                    // Do nothing...
                }
                else if (this.action.ActionType == StreamingActionTypeEnum.SceneCollection)
                {
                    this.SceneCollectionNameTextBox.Text = this.action.SceneCollectionName;
                }
                else
                {
                    this.SourceSceneNameTextBox.Text = this.action.SceneName;
                    this.SourceNameTextBox.Text = this.action.SourceName;
                    this.SourceVisibleCheckBox.IsChecked = this.action.SourceVisible;
                    if (this.action.ActionType == StreamingActionTypeEnum.TextSource)
                    {
                        this.SourceTextTextBox.Text = this.action.SourceText;
                        this.SourceLoadTextFromTextBox.Text = this.action.SourceTextFilePath;
                    }
                    else if (this.action.ActionType == StreamingActionTypeEnum.WebBrowserSource)
                    {
                        this.SourceWebPageTextBox.Text = this.action.SourceURL;
                    }
                    else if (this.action.ActionType == StreamingActionTypeEnum.SourceDimensions)
                    {
                        this.SourceDimensionsXPositionTextBox.Text = this.action.SourceDimensions.X.ToString();
                        this.SourceDimensionsYPositionTextBox.Text = this.action.SourceDimensions.Y.ToString();
                        this.SourceDimensionsRotationTextBox.Text = this.action.SourceDimensions.Rotation.ToString();
                        this.SourceDimensionsXScaleTextBox.Text = this.action.SourceDimensions.XScale.ToString();
                        this.SourceDimensionsYScaleTextBox.Text = this.action.SourceDimensions.YScale.ToString();
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
                    return StreamingSoftwareAction.CreateSceneAction(software, this.SceneNameTextBox.Text);
                }
                else if (type == StreamingActionTypeEnum.StartStopStream)
                {
                    return StreamingSoftwareAction.CreateStartStopStreamAction(software);
                }
                else if (type == StreamingActionTypeEnum.SaveReplayBuffer)
                {
                    return StreamingSoftwareAction.CreateSaveReplayBufferAction(software);
                }
                else if (type == StreamingActionTypeEnum.SceneCollection && !string.IsNullOrEmpty(this.SceneCollectionNameTextBox.Text))
                {
                    return StreamingSoftwareAction.CreateSceneCollectionAction(software, this.SceneCollectionNameTextBox.Text);
                }
                else if (!string.IsNullOrEmpty(this.SourceNameTextBox.Text))
                {
                    if (type == StreamingActionTypeEnum.TextSource)
                    {
                        if (!string.IsNullOrEmpty(this.SourceTextTextBox.Text) && !string.IsNullOrEmpty(this.SourceLoadTextFromTextBox.Text))
                        {
                            StreamingSoftwareAction action = StreamingSoftwareAction.CreateTextSourceAction(software, this.SourceSceneNameTextBox.Text, 
                                this.SourceNameTextBox.Text, this.SourceVisibleCheckBox.IsChecked.GetValueOrDefault(), this.SourceTextTextBox.Text, this.SourceLoadTextFromTextBox.Text);
                            action.UpdateReferenceTextFile(string.Empty);
                            return action;
                        }
                    }
                    else if (type == StreamingActionTypeEnum.WebBrowserSource)
                    {
                        if (!string.IsNullOrEmpty(this.SourceWebPageTextBox.Text))
                        {
                            return StreamingSoftwareAction.CreateWebBrowserSourceAction(software, this.SourceSceneNameTextBox.Text, this.SourceNameTextBox.Text,
                                this.SourceVisibleCheckBox.IsChecked.GetValueOrDefault(), this.SourceWebPageTextBox.Text);
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
                            return StreamingSoftwareAction.CreateSourceDimensionsAction(software, this.SourceSceneNameTextBox.Text, this.SourceNameTextBox.Text,
                                this.SourceVisibleCheckBox.IsChecked.GetValueOrDefault(),
                                new StreamingSourceDimensions() { X = x, Y = y, Rotation = rotation, XScale = xScale, YScale = yScale });
                        }
                    }
                    else
                    {
                        return StreamingSoftwareAction.CreateSourceVisibilityAction(software, this.SourceSceneNameTextBox.Text, this.SourceNameTextBox.Text,
                            this.SourceVisibleCheckBox.IsChecked.GetValueOrDefault());
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
                }
                else if (software == StreamingSoftwareTypeEnum.XSplit)
                {
                    this.XSplitNotEnabledWarningTextBlock.Visibility = (ChannelSession.Services.XSplitServer == null) ? Visibility.Visible : Visibility.Collapsed;
                }
                else if (software == StreamingSoftwareTypeEnum.StreamlabsOBS)
                {
                    this.StreamlabsOBSNotEnabledWarningTextBlock.Visibility = (ChannelSession.Services.StreamlabsOBSService == null) ? Visibility.Visible : Visibility.Collapsed;
                }
                this.StreamingActionTypeComboBox.SelectedIndex = -1;
            }
        }

        private async void StreamingActionTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.FeatureNotSupportedGrid.Visibility = Visibility.Collapsed;
            this.SceneGrid.Visibility = Visibility.Collapsed;
            this.SceneCollectionGrid.Visibility = Visibility.Collapsed;
            this.SourceGrid.Visibility = Visibility.Collapsed;
            this.SourceTextGrid.Visibility = Visibility.Collapsed;
            this.SourceWebBrowserGrid.Visibility = Visibility.Collapsed;
            this.SourceDimensionsGrid.Visibility = Visibility.Collapsed;
            this.ReplayBufferNotEnabledInSettingsGrid.Visibility = Visibility.Collapsed;

            if (this.StreamingActionTypeComboBox.SelectedIndex >= 0)
            {
                StreamingSoftwareTypeEnum software = this.GetSelectedSoftware();
                StreamingActionTypeEnum type = EnumHelper.GetEnumValueFromString<StreamingActionTypeEnum>((string)this.StreamingActionTypeComboBox.SelectedItem);
                if (type == StreamingActionTypeEnum.Scene)
                {
                    this.SceneGrid.Visibility = Visibility.Visible;
                }
                else if (type == StreamingActionTypeEnum.StartStopStream)
                {
                    // Do nothing...
                }
                else if (type == StreamingActionTypeEnum.SaveReplayBuffer)
                {
                    if (software == StreamingSoftwareTypeEnum.XSplit)
                    {
                        this.FeatureNotSupportedGrid.Visibility = Visibility.Visible;
                        return;
                    }
                    else
                    {
                        if (ChannelSession.Services.OBSWebsocket != null)
                        {
                            if (!(await ChannelSession.Services.OBSWebsocket.StartReplayBuffer()))
                            {
                                this.ReplayBufferNotEnabledInSettingsGrid.Visibility = Visibility.Visible;
                                return;
                            }
                        }
                        else if (ChannelSession.Services.StreamlabsOBSService != null)
                        {
                            if (!(await ChannelSession.Services.StreamlabsOBSService.StartReplayBuffer()))
                            {
                                this.ReplayBufferNotEnabledInSettingsGrid.Visibility = Visibility.Visible;
                                return;
                            }
                        }
                    }
                }
                else if (type == StreamingActionTypeEnum.SceneCollection)
                {
                    if (software != StreamingSoftwareTypeEnum.OBSStudio)
                    {
                        this.FeatureNotSupportedGrid.Visibility = Visibility.Visible;
                        return;
                    }
                    else
                    {
                        this.SceneCollectionGrid.Visibility = Visibility.Visible;
                    }
                }
                else
                {
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
                        if (software == StreamingSoftwareTypeEnum.XSplit)
                        {
                            this.FeatureNotSupportedGrid.Visibility = Visibility.Visible;
                            return;
                        }
                        else
                        {
                            this.SourceDimensionsGrid.Visibility = Visibility.Visible;
                        }
                    }
                    this.SourceGrid.Visibility = Visibility.Visible;
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
            string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog(ChannelSession.Services.FileService.HTMLFileFilter());
            if (!string.IsNullOrEmpty(filePath))
            {
                this.SourceWebPageTextBox.Text = filePath;
            }
        }

        private async void GetSourcesDimensionsButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.StreamingSoftwareComboBox.SelectedIndex >= 0 && !string.IsNullOrEmpty(this.SourceNameTextBox.Text))
            {
                StreamingSourceDimensions dimensions = null;

                StreamingSoftwareTypeEnum software = this.GetSelectedSoftware();
                if (software == StreamingSoftwareTypeEnum.OBSStudio)
                {
                    if (ChannelSession.Services.OBSWebsocket != null || await ChannelSession.Services.InitializeOBSWebsocket())
                    {
                        dimensions = await ChannelSession.Services.OBSWebsocket.GetSourceDimensions(this.SourceSceneNameTextBox.Text, this.SourceNameTextBox.Text);
                    }
                    else
                    {
                        await DialogHelper.ShowMessage("Could not connect to OBS Studio. Please try establishing connection with it in the Services area.");
                    }
                }
                else if (software == StreamingSoftwareTypeEnum.StreamlabsOBS)
                {
                    if (ChannelSession.Services.StreamlabsOBSService != null || await ChannelSession.Services.InitializeStreamlabsOBSService())
                    {
                        dimensions = await ChannelSession.Services.StreamlabsOBSService.GetSourceDimensions(this.SourceSceneNameTextBox.Text, this.SourceNameTextBox.Text);
                    }
                    else
                    {
                        await DialogHelper.ShowMessage("Could not connect to OBS Studio. Please try establishing connection with it in the Services area.");
                    }
                }

                if (dimensions != null)
                {
                    this.SourceDimensionsXPositionTextBox.Text = dimensions.X.ToString();
                    this.SourceDimensionsYPositionTextBox.Text = dimensions.Y.ToString();
                    this.SourceDimensionsRotationTextBox.Text = dimensions.Rotation.ToString();
                    this.SourceDimensionsXScaleTextBox.Text = dimensions.XScale.ToString();
                    this.SourceDimensionsYScaleTextBox.Text = dimensions.YScale.ToString();
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
