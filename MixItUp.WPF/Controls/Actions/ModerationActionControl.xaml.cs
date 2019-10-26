using Mixer.Base.Util;
using MixItUp.Base.Actions;
using StreamingClient.Base.Util;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for ModerationActionControl.xaml
    /// </summary>
    public partial class ModerationActionControl : ActionControlBase
    {
        private ModerationAction action;

        public ModerationActionControl() : base() { InitializeComponent(); }

        public ModerationActionControl(ModerationAction action) : this() { this.action = action; }

        public override Task OnLoaded()
        {
            this.ModerationActionTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<ModerationActionTypeEnum>().OrderBy(s => s);

            if (this.action != null)
            {
                this.ModerationActionTypeComboBox.Text = EnumHelper.GetEnumName(this.action.ModerationType);
                this.UserNameTextBox.Text = this.action.UserName;
                this.TimeAmountTextBox.Text = this.action.TimeAmount;
                this.ModerationReasonTextBox.Text = this.action.ModerationReason;
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (this.ModerationActionTypeComboBox.SelectedIndex >= 0)
            {
                ModerationActionTypeEnum moderationType = EnumHelper.GetEnumValueFromString<ModerationActionTypeEnum>((string)this.ModerationActionTypeComboBox.SelectedItem);

                if (moderationType == ModerationActionTypeEnum.ChatTimeout || moderationType == ModerationActionTypeEnum.InteractiveTimeout)
                {
                    if (string.IsNullOrEmpty(this.TimeAmountTextBox.Text))
                    {
                        return null;
                    }
                }

                return new ModerationAction(moderationType, this.UserNameTextBox.Text, this.TimeAmountTextBox.Text, this.ModerationReasonTextBox.Text);
            }
            return null;
        }

        private void ModerationActionTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            this.UserNameTextBox.Visibility = Visibility.Collapsed;
            this.TimeAmountTextBox.Visibility = Visibility.Collapsed;
            this.ModerationReasonTextBox.Visibility = Visibility.Collapsed;

            if (this.ModerationActionTypeComboBox.SelectedIndex >= 0)
            {
                ModerationActionTypeEnum moderationType = EnumHelper.GetEnumValueFromString<ModerationActionTypeEnum>((string)this.ModerationActionTypeComboBox.SelectedItem);

                if (moderationType == ModerationActionTypeEnum.ChatTimeout || moderationType == ModerationActionTypeEnum.PurgeUser ||
                    moderationType == ModerationActionTypeEnum.BanUser || moderationType == ModerationActionTypeEnum.UnbanUser || moderationType == ModerationActionTypeEnum.InteractiveTimeout ||
                    moderationType == ModerationActionTypeEnum.AddModerationStrike || moderationType == ModerationActionTypeEnum.RemoveModerationStrike)
                {
                    this.UserNameTextBox.Visibility = Visibility.Visible;
                }

                if (moderationType == ModerationActionTypeEnum.ChatTimeout || moderationType == ModerationActionTypeEnum.InteractiveTimeout)
                {
                    this.TimeAmountTextBox.Visibility = Visibility.Visible;
                }

                if (moderationType == ModerationActionTypeEnum.AddModerationStrike)
                {
                    this.ModerationReasonTextBox.Visibility = Visibility.Visible;
                }
            }
        }
    }
}
