using System.Windows.Controls;
using System.Windows;
using Mixer.Base.Util;
using Microsoft.Win32;
using MixItUp.Base.Actions;

namespace MixItUp.WPF.Controls.Commands
{
    /// <summary>
    /// Interaction logic for ActionControl.xaml
    /// </summary>
    public partial class ActionControl : UserControl
    {
        public ActionControl()
        {
            InitializeComponent();

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
    }
}
