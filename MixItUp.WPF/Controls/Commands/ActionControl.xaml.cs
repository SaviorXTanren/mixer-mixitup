using System.Windows.Controls;
using System.Windows;
using Mixer.Base.Util;
using Microsoft.Win32;
using MixItUp.Base.Actions;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.WPF.Controls.Commands
{
    /// <summary>
    /// Interaction logic for ActionControl.xaml
    /// </summary>
    public partial class ActionControl : UserControl
    {
        private ActionBase Action;

        public ActionControl() : this(null) { }

        public ActionControl(ActionBase action)
        {
            InitializeComponent();

            this.Action = action;

            this.Loaded += ActionControl_Loaded;
        }

        public ActionBase GetAction()
        {
            if (this.TypeComboBox.SelectedIndex >= 0)
            {
                string typeName = (string)this.TypeComboBox.SelectedItem;
                ActionTypeEnum type = EnumHelper.GetEnumValueFromString<ActionTypeEnum>(typeName);

                switch (type)
                {
                    case ActionTypeEnum.Chat:
                        if (!string.IsNullOrEmpty(this.ChatMessageTextBox.Text))
                        {
                            return new ChatAction(this.ChatMessageTextBox.Text);
                        }
                        break;
                    case ActionTypeEnum.Cooldown:
                        break;
                    case ActionTypeEnum.Currency:
                        int currencyAmount;
                        if (!string.IsNullOrEmpty(this.CurrencyAmountTextBox.Text) && int.TryParse(this.CurrencyAmountTextBox.Text, out currencyAmount))
                        {
                            return new CurrencyAction(currencyAmount);
                        }
                        break;
                    case ActionTypeEnum.ExternalProgram:
                        if (!string.IsNullOrEmpty(this.ProgramFilePathTextBox.Text))
                        {
                            return new ExternalProgramAction(this.ProgramFilePathTextBox.Text, this.ProgramArgumentsTextBox.Text, showWindow: true);
                        }
                        break;
                    case ActionTypeEnum.Giveaway:
                        break;
                    case ActionTypeEnum.Input:
                        if (this.InputButtonComboBox.SelectedIndex >= 0)
                        {
                            return new InputAction(new List<InputTypeEnum>() { EnumHelper.GetEnumValueFromString<InputTypeEnum>((string)this.InputButtonComboBox.SelectedItem) });
                        }
                        break;
                    case ActionTypeEnum.Overlay:
                        break;
                    case ActionTypeEnum.Sound:
                        if (!string.IsNullOrEmpty(this.SoundFilePathTextBox.Text) && !string.IsNullOrEmpty(this.SoundVolumeTextBox.Text))
                        {
                            int volumeLevel;
                            if (int.TryParse(this.SoundVolumeTextBox.Text, out volumeLevel) && volumeLevel >= 0 && volumeLevel <= 100)
                            {
                                return new SoundAction(this.SoundFilePathTextBox.Text, volumeLevel / 100);
                            }
                        }
                        break;
                    case ActionTypeEnum.Whisper:
                        if (!string.IsNullOrEmpty(this.WhisperMessageTextBox.Text))
                        {
                            return new WhisperAction(this.WhisperMessageTextBox.Text);
                        }
                        break;
                }
            }
            return null;
        }

        private void ActionControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.TypeComboBox.ItemsSource = EnumHelper.GetEnumNames<ActionTypeEnum>();

            if (this.Action != null)
            {
                this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(this.Action.Type);
                switch (this.Action.Type)
                {
                    case ActionTypeEnum.Chat:
                        ChatAction chatAction = (ChatAction)this.Action;
                        this.ChatMessageTextBox.Text = chatAction.ChatText;
                        break;
                    case ActionTypeEnum.Cooldown:
                        CooldownAction cooldownAction = (CooldownAction)this.Action;
                        
                        break;
                    case ActionTypeEnum.Currency:
                        CurrencyAction currencyAction = (CurrencyAction)this.Action;
                        this.CurrencyAmountTextBox.Text = currencyAction.Amount.ToString();
                        break;
                    case ActionTypeEnum.ExternalProgram:
                        ExternalProgramAction externalAction = (ExternalProgramAction)this.Action;
                        this.ProgramFilePathTextBox.Text = externalAction.FilePath;
                        this.ProgramArgumentsTextBox.Text = externalAction.Arguments;
                        break;
                    case ActionTypeEnum.Giveaway:
                        GiveawayAction giveawayAction = (GiveawayAction)this.Action;
                        break;
                    case ActionTypeEnum.Input:
                        InputAction inputAction = (InputAction)this.Action;
                        this.InputButtonComboBox.SelectedItem = EnumHelper.GetEnumName(inputAction.Inputs.First());
                        break;
                    case ActionTypeEnum.Overlay:
                        OverlayAction overlayAction = (OverlayAction)this.Action;
                        this.OverlayImageFilePathTextBox.Text = overlayAction.FilePath;
                        this.OverlayDurationTextBox.Text = overlayAction.Duration.ToString();
                        this.OverlayHorizontalTextBox.Text = overlayAction.Horizontal.ToString();
                        this.OverlayVerticalTextBox.Text = overlayAction.Vertical.ToString();
                        break;
                    case ActionTypeEnum.Sound:
                        SoundAction soundAction = (SoundAction)this.Action;
                        this.SoundFilePathTextBox.Text = soundAction.FilePath;
                        int volume = (int)(soundAction.VolumeScale * 100);
                        this.SoundVolumeTextBox.Text = volume.ToString();
                        break;
                    case ActionTypeEnum.Whisper:
                        WhisperAction whisperAction = (WhisperAction)this.Action;
                        this.WhisperMessageTextBox.Text = whisperAction.ChatText;
                        break;
                }

                this.Action = null;
            }
        }

        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.ChatGrid.Visibility = Visibility.Hidden;
            this.CooldownGrid.Visibility = Visibility.Hidden;
            this.CurrencyGrid.Visibility = Visibility.Hidden;
            this.ExternalProgramGrid.Visibility = Visibility.Hidden;
            this.GiveawayGrid.Visibility = Visibility.Hidden;
            this.InputGrid.Visibility = Visibility.Hidden;
            this.OverlayGrid.Visibility = Visibility.Hidden;
            this.SoundGrid.Visibility = Visibility.Hidden;
            this.WhisperGrid.Visibility = Visibility.Hidden;

            if (this.TypeComboBox.SelectedIndex >= 0)
            {
                string typeName = (string)this.TypeComboBox.SelectedItem;
                ActionTypeEnum type = EnumHelper.GetEnumValueFromString<ActionTypeEnum>(typeName);

                switch (type)
                {
                    case ActionTypeEnum.Chat:
                        this.ChatGrid.Visibility = Visibility.Visible;
                        break;
                    case ActionTypeEnum.Cooldown:
                        this.CooldownGrid.Visibility = Visibility.Visible;
                        break;
                    case ActionTypeEnum.Currency:
                        this.CurrencyGrid.Visibility = Visibility.Visible;
                        break;
                    case ActionTypeEnum.ExternalProgram:
                        this.ExternalProgramGrid.Visibility = Visibility.Visible;
                        break;
                    case ActionTypeEnum.Giveaway:
                        this.GiveawayGrid.Visibility = Visibility.Visible;
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
                    case ActionTypeEnum.Whisper:
                        this.WhisperGrid.Visibility = Visibility.Visible;
                        break;
                }
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
    }
}
