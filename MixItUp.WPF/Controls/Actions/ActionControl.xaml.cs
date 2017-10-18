using Microsoft.Win32;
using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Util;
using MixItUp.WPF.Windows.Command;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for ActionControl.xaml
    /// </summary>
    public partial class ActionControl : UserControl
    {
        private const int MinimzedGroupBoxHeight = 35;

        private CommandWindow window;

        private ActionTypeEnum type;
        private ActionBase action;

        public ActionControl(CommandWindow window, ActionTypeEnum type)
        {
            this.window = window;
            this.type = type;

            InitializeComponent();

            this.Loaded += ActionControl_Loaded;
        }

        public ActionControl(CommandWindow window, ActionBase action) : this(window, action.Type) { this.action = action; }

        public ActionBase GetAction()
        {
            switch (this.type)
            {
                case ActionTypeEnum.Chat:
                    if (!string.IsNullOrEmpty(this.ChatMessageTextBox.Text))
                    {
                        return new ChatAction(this.ChatMessageTextBox.Text, this.ChatWhisperToggleButton.IsChecked.GetValueOrDefault());
                    }
                    break;
                case ActionTypeEnum.Currency:
                    int currencyAmount;
                    if (!string.IsNullOrEmpty(this.CurrencyAmountTextBox.Text) && int.TryParse(this.CurrencyAmountTextBox.Text, out currencyAmount) && !string.IsNullOrEmpty(this.CurrencyMessageTextBox.Text))
                    {
                        return new CurrencyAction(currencyAmount, this.CurrencyMessageTextBox.Text, this.CurrencyWhisperToggleButton.IsChecked.GetValueOrDefault());
                    }
                    break;
                case ActionTypeEnum.ExternalProgram:
                    if (!string.IsNullOrEmpty(this.ProgramFilePathTextBox.Text))
                    {
                        return new ExternalProgramAction(this.ProgramFilePathTextBox.Text, this.ProgramArgumentsTextBox.Text, showWindow: true);
                    }
                    break;
                case ActionTypeEnum.Input:
                    if (this.InputButtonComboBox.SelectedIndex >= 0)
                    {
                        return new InputAction(new List<InputTypeEnum>() { EnumHelper.GetEnumValueFromString<InputTypeEnum>((string)this.InputButtonComboBox.SelectedItem) });
                    }
                    break;
                case ActionTypeEnum.Overlay:
                    if (!string.IsNullOrEmpty(this.OverlayImageFilePathTextBox.Text) || !(string.IsNullOrEmpty(this.OverlayTextTextBox.Text)))
                    {
                        double duration;
                        int horizontal;
                        int vertical;
                        if (double.TryParse(this.OverlayDurationTextBox.Text, out duration) && duration > 0 &&
                            int.TryParse(this.OverlayHorizontalTextBox.Text, out horizontal) && horizontal >= 0 && horizontal <= 100 &&
                            int.TryParse(this.OverlayVerticalTextBox.Text, out vertical) && vertical >= 0 && vertical <= 100)
                        {
                            if (!string.IsNullOrEmpty(this.OverlayImageFilePathTextBox.Text))
                            {
                                int width;
                                int height;
                                if (int.TryParse(this.OverlayImageWidthTextBox.Text, out width) && width > 0 &&
                                    int.TryParse(this.OverlayImageHeightTextBox.Text, out height) && height > 0)
                                {
                                    return new OverlayAction(this.OverlayImageFilePathTextBox.Text, width, height, duration, horizontal, vertical);
                                }
                            }
                            else if (!string.IsNullOrEmpty(this.OverlayTextTextBox.Text) && !string.IsNullOrEmpty(this.OverlayFontColorTextBox.Text))
                            {
                                int fontSize;
                                if (int.TryParse(this.OverlayFontSizeTextBox.Text, out fontSize) && fontSize > 0)
                                {
                                    return new OverlayAction(this.OverlayTextTextBox.Text, this.OverlayFontColorTextBox.Text, fontSize, duration, horizontal, vertical);
                                }
                            }
                        }
                    }
                    break;
                case ActionTypeEnum.Sound:
                    if (!string.IsNullOrEmpty(this.SoundFilePathTextBox.Text) && !string.IsNullOrEmpty(this.SoundVolumeTextBox.Text))
                    {
                        int volumeLevel;
                        if (int.TryParse(this.SoundVolumeTextBox.Text, out volumeLevel) && volumeLevel >= 0 && volumeLevel <= 100)
                        {
                            return new SoundAction(this.SoundFilePathTextBox.Text, volumeLevel);
                        }
                    }
                    break;
                case ActionTypeEnum.Wait:
                    double waitAmount;
                    if (!string.IsNullOrEmpty(this.WaitAmountTextBox.Text) && double.TryParse(this.WaitAmountTextBox.Text, out waitAmount) && waitAmount > 0.0)
                    {
                        return new WaitAction(waitAmount);
                    }
                    break;
                case ActionTypeEnum.OBSStudio:
                    if (this.OBSStudioTypeComboBox.SelectedIndex >= 0)
                    {
                        if (this.OBSStudioSceneGrid.Visibility == Visibility.Visible && !string.IsNullOrEmpty(this.OBSStudioSceneNameTextBox.Text))
                        {
                            return new OBSStudioAction(this.OBSStudioSceneCollectionNameTextBox.Text, this.OBSStudioSceneNameTextBox.Text);
                        }
                        else if (this.OBSStudioSourceGrid.Visibility == Visibility.Visible && !string.IsNullOrEmpty(this.OBSStudioSourceNameTextBox.Text))
                        {
                            OBSStudioAction action = new OBSStudioAction(this.OBSStudioSourceNameTextBox.Text, this.OBSStudioSourceVisibleCheckBox.IsChecked.GetValueOrDefault(), this.OBSStudioSourceTextTextBox.Text);
                            action.UpdateReferenceTextFile();
                            return action;
                        }
                    }
                    break;
                case ActionTypeEnum.XSplit:
                    if (this.XSplitTypeComboBox.SelectedIndex >= 0)
                    {
                        if (this.XSplitSceneGrid.Visibility == Visibility.Visible && !string.IsNullOrEmpty(this.XSplitSceneNameTextBox.Text))
                        {
                            return new XSplitAction(this.XSplitSceneNameTextBox.Text);
                        }
                        else if (this.XSplitSourceGrid.Visibility == Visibility.Visible && !string.IsNullOrEmpty(this.XSplitSourceNameTextBox.Text))
                        {
                            XSplitAction action = new XSplitAction(this.XSplitSourceNameTextBox.Text, this.XSplitSourceVisibleCheckBox.IsChecked.GetValueOrDefault(), this.XSplitSourceTextTextBox.Text);
                            action.UpdateReferenceTextFile();
                            return action;
                        }
                    }
                    break;
                case ActionTypeEnum.Counter:
                    int counterAmount;
                    if (!string.IsNullOrEmpty(this.CounterNameTextBox.Text) && this.CounterNameTextBox.Text.All(c => char.IsLetterOrDigit(c)) &&
                        !string.IsNullOrEmpty(this.CounterAmountTextBox.Text) && int.TryParse(this.CounterAmountTextBox.Text, out counterAmount))
                    {
                        return new CounterAction(this.CounterNameTextBox.Text, counterAmount);
                    }
                    break;
                case ActionTypeEnum.GameQueue:
                    if (this.GameQueueActionTypeComboBox.SelectedIndex >= 0)
                    {
                        GameQueueActionType gameQueueType = EnumHelper.GetEnumValueFromString<GameQueueActionType>((string)this.GameQueueActionTypeComboBox.SelectedItem);
                        return new GameQueueAction(gameQueueType);
                    }
                    break;
            }
            return null;
        }

        public void Expand() { this.GroupBox.Height = Double.NaN; }

        public void Minimize() { this.GroupBox.Height = MinimzedGroupBoxHeight; }

        private void ActionControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.GroupBoxHeaderTextBox.Text = EnumHelper.GetEnumName(this.type);

            this.OverlayTypeComboBox.ItemsSource = new List<string>() { "Image", "Text" };
            this.OBSStudioTypeComboBox.ItemsSource = new List<string>() { "Scene", "Source" };
            this.XSplitTypeComboBox.ItemsSource = new List<string>() { "Scene", "Source" };
            this.GameQueueActionTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<GameQueueActionType>();

            switch (type)
            {
                case ActionTypeEnum.Chat:
                    this.ChatGrid.Visibility = Visibility.Visible;
                    break;
                case ActionTypeEnum.Currency:
                    this.CurrencyGrid.Visibility = Visibility.Visible;
                    break;
                case ActionTypeEnum.ExternalProgram:
                    this.ExternalProgramGrid.Visibility = Visibility.Visible;
                    break;
                case ActionTypeEnum.Input:
                    this.InputGrid.Visibility = Visibility.Visible;
                    break;
                case ActionTypeEnum.Overlay:
                    this.OverlayGrid.Visibility = Visibility.Visible;
                    if (!ChannelSession.Settings.EnableOverlay)
                    {
                        this.OverlayNotEnabledWarningTextBlock.Visibility = Visibility.Visible;
                    }
                    break;
                case ActionTypeEnum.Sound:
                    this.SoundGrid.Visibility = Visibility.Visible;
                    break;
                case ActionTypeEnum.Wait:
                    this.WaitGrid.Visibility = Visibility.Visible;
                    break;
                case ActionTypeEnum.OBSStudio:
                    this.OBSStudioGrid.Visibility = Visibility.Visible;
                    if (string.IsNullOrEmpty(ChannelSession.Settings.OBSStudioServerIP))
                    {
                        this.OBSStudioNotEnabledWarningTextBlock.Visibility = Visibility.Visible;
                    }
                    break;
                case ActionTypeEnum.XSplit:
                    this.XSplitGrid.Visibility = Visibility.Visible;
                    if (!ChannelSession.Settings.EnableXSplitConnection)
                    {
                        this.XSplitNotEnabledWarningTextBlock.Visibility = Visibility.Visible;
                    }
                    break;
                case ActionTypeEnum.Counter:
                    this.CounterGrid.Visibility = Visibility.Visible;
                    break;
                case ActionTypeEnum.GameQueue:
                    this.GameQueueGrid.Visibility = Visibility.Visible;
                    break;
            }

            if (this.action != null)
            {
                switch (this.type)
                {
                    case ActionTypeEnum.Chat:
                        ChatAction chatAction = (ChatAction)this.action;
                        this.ChatMessageTextBox.Text = chatAction.ChatText;
                        this.ChatWhisperToggleButton.IsChecked = chatAction.IsWhisper;
                        break;
                    case ActionTypeEnum.Currency:
                        CurrencyAction currencyAction = (CurrencyAction)this.action;
                        this.CurrencyAmountTextBox.Text = currencyAction.Amount.ToString();
                        this.CurrencyMessageTextBox.Text = currencyAction.ChatText;
                        this.CurrencyWhisperToggleButton.IsChecked = currencyAction.IsWhisper;
                        break;
                    case ActionTypeEnum.ExternalProgram:
                        ExternalProgramAction externalAction = (ExternalProgramAction)this.action;
                        this.ProgramFilePathTextBox.Text = externalAction.FilePath;
                        this.ProgramArgumentsTextBox.Text = externalAction.Arguments;
                        break;
                    case ActionTypeEnum.Input:
                        InputAction inputAction = (InputAction)this.action;
                        this.InputButtonComboBox.SelectedItem = EnumHelper.GetEnumName(inputAction.Inputs.First());
                        break;
                    case ActionTypeEnum.Overlay:
                        OverlayAction overlayAction = (OverlayAction)this.action;
                        if (!string.IsNullOrEmpty(overlayAction.ImagePath))
                        {
                            this.OverlayTypeComboBox.SelectedItem = "Image";
                            this.OverlayImageFilePathTextBox.Text = overlayAction.ImagePath;
                            this.OverlayImageWidthTextBox.Text = overlayAction.ImageWidth.ToString();
                            this.OverlayImageHeightTextBox.Text = overlayAction.ImageHeight.ToString();
                        }
                        else if (!string.IsNullOrEmpty(overlayAction.Text))
                        {
                            this.OverlayTypeComboBox.SelectedItem = "Text";
                            this.OverlayTextTextBox.Text = overlayAction.Text;
                            this.OverlayFontSizeTextBox.Text = overlayAction.FontSize.ToString();
                            this.OverlayFontColorTextBox.Text = overlayAction.Color;
                        }
                        this.OverlayDurationTextBox.Text = overlayAction.Duration.ToString();
                        this.OverlayHorizontalTextBox.Text = overlayAction.Horizontal.ToString();
                        this.OverlayVerticalTextBox.Text = overlayAction.Vertical.ToString();
                        break;
                    case ActionTypeEnum.Sound:
                        SoundAction soundAction = (SoundAction)this.action;
                        this.SoundFilePathTextBox.Text = soundAction.FilePath;
                        this.SoundVolumeTextBox.Text = soundAction.VolumeScale.ToString();
                        break;
                    case ActionTypeEnum.Wait:
                        WaitAction waitAction = (WaitAction)this.action;
                        this.WaitAmountTextBox.Text = waitAction.WaitAmount.ToString();
                        break;
                    case ActionTypeEnum.OBSStudio:
                        OBSStudioAction obsStudioAction = (OBSStudioAction)this.action;
                        if (!string.IsNullOrEmpty(obsStudioAction.SceneName))
                        {
                            this.OBSStudioTypeComboBox.SelectedItem = "Scene";
                            this.OBSStudioSceneCollectionNameTextBox.Text = obsStudioAction.SceneCollection;
                            this.OBSStudioSceneNameTextBox.Text = obsStudioAction.SceneName;
                        }
                        else
                        {
                            this.OBSStudioTypeComboBox.SelectedItem = "Source";
                            this.OBSStudioSourceNameTextBox.Text = obsStudioAction.SourceName;
                            this.OBSStudioSourceVisibleCheckBox.IsChecked = obsStudioAction.SourceVisible;
                            this.OBSStudioSourceTextTextBox.Text = obsStudioAction.SourceText;
                            this.OBSStudioSourceLoadTextFromTextBox.Text = obsStudioAction.LoadTextFromFilePath;
                        }
                        break;
                    case ActionTypeEnum.XSplit:
                        XSplitAction xsplitAction = (XSplitAction)this.action;
                        if (!string.IsNullOrEmpty(xsplitAction.SceneName))
                        {
                            this.XSplitTypeComboBox.SelectedItem = "Scene";
                            this.XSplitSceneNameTextBox.Text = xsplitAction.SceneName;
                        }
                        else
                        {
                            this.XSplitTypeComboBox.SelectedItem = "Source";
                            this.XSplitSourceNameTextBox.Text = xsplitAction.SourceName;
                            this.XSplitSourceVisibleCheckBox.IsChecked = xsplitAction.SourceVisible;
                            this.XSplitSourceTextTextBox.Text = xsplitAction.SourceText;
                            this.XSplitSourceLoadTextFromTextBox.Text = xsplitAction.LoadTextFromFilePath;
                        }
                        break;
                    case ActionTypeEnum.Counter:
                        CounterAction counterAction = (CounterAction)this.action;
                        this.CounterNameTextBox.Text = counterAction.CounterName;
                        this.CounterAmountTextBox.Text = counterAction.CounterAmount.ToString();
                        break;
                    case ActionTypeEnum.GameQueue:
                        GameQueueAction gameQueueAction = (GameQueueAction)this.action;
                        this.GameQueueActionTypeComboBox.SelectedItem = EnumHelper.GetEnumName(gameQueueAction.GameQueueType);
                        break;
                }

                this.action = null;
            }
        }

        private void MoveUpActionButton_Click(object sender, RoutedEventArgs e)
        {
            this.window.MoveActionUp(this);
        }

        private void MoveDownActionButton_Click(object sender, RoutedEventArgs e)
        {
            this.window.MoveActionDown(this);
        }

        private void DeleteActionButton_Click(object sender, RoutedEventArgs e)
        {
            this.window.DeleteAction(this);
        }

        private void SoundFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = FileSystemHelper.ShowOpenFileDialog("MP3 Files (*.mp3)|*.mp3|All files (*.*)|*.*");
            if (!string.IsNullOrEmpty(filePath))
            {
                this.SoundFilePathTextBox.Text = filePath;
            }
        }

        private void ProgramFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = FileSystemHelper.ShowOpenFileDialog();
            if (!string.IsNullOrEmpty(filePath))
            {
                this.ProgramFilePathTextBox.Text = filePath;
            }
        }

        private void OverlayImageFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = FileSystemHelper.ShowOpenFileDialog();
            if (!string.IsNullOrEmpty(filePath))
            {
                this.OverlayImageFilePathTextBox.Text = filePath;
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        private void OverlayTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.OverlayImageGrid.Visibility = Visibility.Hidden;
            this.OverlayTextGrid.Visibility = Visibility.Hidden;
            if (this.OverlayTypeComboBox.SelectedIndex >= 0)
            {
                if (this.OverlayTypeComboBox.SelectedItem.ToString().Equals("Image"))
                {
                    this.OverlayImageGrid.Visibility = Visibility.Visible;
                }
                else if (this.OverlayTypeComboBox.SelectedItem.ToString().Equals("Text"))
                {
                    this.OverlayTextGrid.Visibility = Visibility.Visible;
                }
                this.OverlayPositionGrid.Visibility = Visibility.Visible;
            }
        }

        private void OBSStudioTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.OBSStudioSceneGrid.Visibility = Visibility.Hidden;
            this.OBSStudioSourceGrid.Visibility = Visibility.Hidden;
            if (this.OBSStudioTypeComboBox.SelectedIndex >= 0)
            {
                if (this.OBSStudioTypeComboBox.SelectedItem.ToString().Equals("Scene"))
                {
                    this.OBSStudioSceneGrid.Visibility = Visibility.Visible;
                }
                else if (this.OBSStudioTypeComboBox.SelectedItem.ToString().Equals("Source"))
                {
                    this.OBSStudioSourceGrid.Visibility = Visibility.Visible;
                }
            }
        }

        private void XSplitTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.XSplitSceneGrid.Visibility = Visibility.Hidden;
            this.XSplitSourceGrid.Visibility = Visibility.Hidden;
            if (this.XSplitTypeComboBox.SelectedIndex >= 0)
            {
                if (this.XSplitTypeComboBox.SelectedItem.ToString().Equals("Scene"))
                {
                    this.XSplitSceneGrid.Visibility = Visibility.Visible;
                }
                else if (this.XSplitTypeComboBox.SelectedItem.ToString().Equals("Source"))
                {
                    this.XSplitSourceGrid.Visibility = Visibility.Visible;
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

        private void XSplitSourceNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.XSplitSourceNameTextBox.Text))
            {
                this.XSplitSourceLoadTextFromTextBox.Text = Path.Combine(XSplitAction.XSplitReferenceTextFilesDirectory, this.XSplitSourceNameTextBox.Text + ".txt");
            }
        }

        private void Grid_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.GroupBox.Height == MinimzedGroupBoxHeight)
            {
                this.Expand();
            }
            else
            {
                this.Minimize();
            }
        }
    }
}
