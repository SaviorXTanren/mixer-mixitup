using Microsoft.Win32;
using Mixer.Base.Util;
using MixItUp.Base.Actions;
using System.Collections.Generic;
using System.Diagnostics;
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
        private ActionTypeEnum type;
        private ActionBase action;

        public ActionControl(ActionTypeEnum type)
        {
            this.type = type;

            InitializeComponent();

            this.Loaded += ActionControl_Loaded;
        }

        public ActionControl(ActionBase action) : this(action.Type) { this.action = action; }

        public ActionBase GetAction()
        {
            switch (this.type)
            {
                case ActionTypeEnum.Chat:
                    if (!string.IsNullOrEmpty(this.ChatMessageTextBox.Text))
                    {
                        return new ChatAction(this.ChatMessageTextBox.Text, this.ChatWhisperCheckBox.IsChecked.GetValueOrDefault());
                    }
                    break;
                case ActionTypeEnum.Currency:
                    int currencyAmount;
                    if (!string.IsNullOrEmpty(this.CurrencyAmountTextBox.Text) && int.TryParse(this.CurrencyAmountTextBox.Text, out currencyAmount) && !string.IsNullOrEmpty(this.CurrencyMessageTextBox.Text))
                    {
                        return new CurrencyAction(currencyAmount, this.CurrencyMessageTextBox.Text, this.CurrencyWhisperCheckBox.IsChecked.GetValueOrDefault());
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
                    if (!string.IsNullOrEmpty(this.OverlayImageFilePathTextBox.Text))
                    {
                        int duration;
                        int horizontal;
                        int vertical;
                        if (int.TryParse(this.OverlayDurationTextBox.Text, out duration) && duration > 0 &&
                            int.TryParse(this.OverlayHorizontalTextBox.Text, out horizontal) && horizontal >= 0 && horizontal <= 100 &&
                            int.TryParse(this.OverlayVerticalTextBox.Text, out vertical) && vertical >= 0 && vertical <= 100)
                        {
                            return new OverlayAction(this.OverlayImageFilePathTextBox.Text, duration, horizontal, vertical);
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
                    int waitAmount;
                    if (!string.IsNullOrEmpty(this.WaitAmountTextBox.Text) && int.TryParse(this.WaitAmountTextBox.Text, out waitAmount) && waitAmount > 0)
                    {
                        return new WaitAction(waitAmount);
                    }
                    break;
            }
            return null;
        }

        private void ActionControl_Loaded(object sender, RoutedEventArgs e)
        {
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
                    break;
                case ActionTypeEnum.Sound:
                    this.SoundGrid.Visibility = Visibility.Visible;
                    break;
                case ActionTypeEnum.Wait:
                    this.WaitGrid.Visibility = Visibility.Visible;
                    break;
            }

            if (this.action != null)
            {
                switch (this.type)
                {
                    case ActionTypeEnum.Chat:
                        ChatAction chatAction = (ChatAction)this.action;
                        this.ChatMessageTextBox.Text = chatAction.ChatText;
                        this.ChatWhisperCheckBox.IsChecked = chatAction.IsWhisper;
                        break;
                    case ActionTypeEnum.Currency:
                        CurrencyAction currencyAction = (CurrencyAction)this.action;
                        this.CurrencyAmountTextBox.Text = currencyAction.Amount.ToString();
                        this.CurrencyMessageTextBox.Text = currencyAction.ChatText;
                        this.CurrencyWhisperCheckBox.IsChecked = currencyAction.IsWhisper;
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
                        this.OverlayImageFilePathTextBox.Text = overlayAction.FilePath;
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
                }

                this.action = null;
            }
        }

        private void SoundFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "MP3 Files (*.mp3)|*.mp3|All files (*.*)|*.*";
            fileDialog.CheckFileExists = true;
            fileDialog.CheckPathExists = true;
            if (fileDialog.ShowDialog() == true)
            {
                this.SoundFilePathTextBox.Text = fileDialog.FileName;
            }
        }

        private void ProgramFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "All files (*.*)|*.*";
            fileDialog.CheckFileExists = true;
            fileDialog.CheckPathExists = true;
            if (fileDialog.ShowDialog() == true)
            {
                this.ProgramFilePathTextBox.Text = fileDialog.FileName;
            }
        }

        private void OverlayImageFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "All files (*.*)|*.*";
            fileDialog.CheckFileExists = true;
            fileDialog.CheckPathExists = true;
            if (fileDialog.ShowDialog() == true)
            {
                this.OverlayImageFilePathTextBox.Text = fileDialog.FileName;
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}
