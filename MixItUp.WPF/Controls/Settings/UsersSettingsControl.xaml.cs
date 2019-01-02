using MixItUp.Base;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Settings
{
    /// <summary>
    /// Interaction logic for UsersSettingsControl.xaml
    /// </summary>
    public partial class UsersSettingsControl : SettingsControlBase
    {
        public UsersSettingsControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            if (ChannelSession.Settings.RegularUserMinimumHours > 0)
            {
                this.RegularUserMinimumHoursTextBox.Text = ChannelSession.Settings.RegularUserMinimumHours.ToString();
            }

            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.InitializeInternal();
        }

        private void RegularUserMinimumHoursTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.RegularUserMinimumHoursTextBox.Text) && int.TryParse(this.RegularUserMinimumHoursTextBox.Text, out int time) && time > 0)
            {
                ChannelSession.Settings.RegularUserMinimumHours = time;
            }
            else
            {
                this.RegularUserMinimumHoursTextBox.Text = string.Empty;
            }
        }
    }
}