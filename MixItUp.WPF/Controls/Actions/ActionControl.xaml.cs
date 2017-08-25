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
        private IEnumerable<ActionTypeEnum> allowedActions;
        private ActionBase Action;

        public ActionControl() : this(null) { }

        public ActionControl(IEnumerable<ActionTypeEnum> allowedActions) : this(allowedActions, null) { }

        public ActionControl(IEnumerable<ActionTypeEnum> allowedActions, ActionBase action)
        {
            InitializeComponent();

            this.allowedActions = allowedActions;
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
                            return new ChatAction(this.ChatMessageTextBox.Text, this.ChatWhisperCheckBox.IsChecked.GetValueOrDefault());
                        }
                        break;
                    case ActionTypeEnum.Cooldown:
                        int cooldownAmount;
                        if (this.CooldownTypeComboBox.SelectedIndex >= 0 && int.TryParse(this.CooldownAmountTextBox.Text, out cooldownAmount) && cooldownAmount > 0)
                        {
                            return new CooldownAction(EnumHelper.GetEnumValueFromString<CooldownActionTypeEnum>((string)this.CooldownTypeComboBox.SelectedItem), cooldownAmount);
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
                    case ActionTypeEnum.Giveaway:
                        if (!string.IsNullOrEmpty(this.GiveawayItemTextBox.Text))
                        {
                            return new GiveawayAction(this.GiveawayItemTextBox.Text);
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
            }
            return null;
        }

        private void ActionControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.allowedActions == null)
            {
                this.TypeComboBox.ItemsSource = EnumHelper.GetEnumNames<ActionTypeEnum>();
            }
            else
            {
                this.TypeComboBox.ItemsSource = EnumHelper.GetEnumNames<ActionTypeEnum>(this.allowedActions);
            }

            this.CooldownTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<CooldownActionTypeEnum>();
            this.InputButtonComboBox.ItemsSource = EnumHelper.GetEnumNames<InputTypeEnum>();

            if (this.Action != null)
            {
                this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(this.Action.Type);
                switch (this.Action.Type)
                {
                    case ActionTypeEnum.Chat:
                        ChatAction chatAction = (ChatAction)this.Action;
                        this.ChatMessageTextBox.Text = chatAction.ChatText;
                        this.ChatWhisperCheckBox.IsChecked = chatAction.IsWhisper;
                        break;
                    case ActionTypeEnum.Cooldown:
                        CooldownAction cooldownAction = (CooldownAction)this.Action;
                        this.CooldownTypeComboBox.SelectedItem = EnumHelper.GetEnumName(cooldownAction.CooldownType);
                        this.CooldownAmountTextBox.Text = cooldownAction.Amount.ToString();
                        break;
                    case ActionTypeEnum.Currency:
                        CurrencyAction currencyAction = (CurrencyAction)this.Action;
                        this.CurrencyAmountTextBox.Text = currencyAction.Amount.ToString();
                        this.CurrencyMessageTextBox.Text = currencyAction.ChatText;
                        this.CurrencyWhisperCheckBox.IsChecked = currencyAction.IsWhisper;
                        break;
                    case ActionTypeEnum.ExternalProgram:
                        ExternalProgramAction externalAction = (ExternalProgramAction)this.Action;
                        this.ProgramFilePathTextBox.Text = externalAction.FilePath;
                        this.ProgramArgumentsTextBox.Text = externalAction.Arguments;
                        break;
                    case ActionTypeEnum.Giveaway:
                        GiveawayAction giveawayAction = (GiveawayAction)this.Action;
                        this.GiveawayItemTextBox.Text = giveawayAction.GiveawayItem;
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
                        this.SoundVolumeTextBox.Text = soundAction.VolumeScale.ToString();
                        break;
                    case ActionTypeEnum.Wait:
                        WaitAction waitAction = (WaitAction)this.Action;
                        this.WaitAmountTextBox.Text = waitAction.WaitAmount.ToString();
                        break;
                }

                this.Action = null;
            }
        }

        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.ChatGrid.Visibility = Visibility.Collapsed;
            this.CooldownGrid.Visibility = Visibility.Collapsed;
            this.CurrencyGrid.Visibility = Visibility.Collapsed;
            this.ExternalProgramGrid.Visibility = Visibility.Collapsed;
            this.GiveawayGrid.Visibility = Visibility.Collapsed;
            this.InputGrid.Visibility = Visibility.Collapsed;
            this.OverlayGrid.Visibility = Visibility.Collapsed;
            this.SoundGrid.Visibility = Visibility.Collapsed;
            this.WaitGrid.Visibility = Visibility.Collapsed;

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
                    case ActionTypeEnum.Wait:
                        this.WaitGrid.Visibility = Visibility.Visible;
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

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}
