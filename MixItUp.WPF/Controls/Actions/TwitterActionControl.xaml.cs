using Mixer.Base.Util;
using MixItUp.Base.Actions;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for TwitterActionControl.xaml
    /// </summary>
    public partial class TwitterActionControl : ActionControlBase
    {
        private TwitterAction action;

        public TwitterActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public TwitterActionControl(ActionContainerControl containerControl, TwitterAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            this.TypeComboBox.ItemsSource = EnumHelper.GetEnumNames<TwitterActionTypeEnum>().OrderBy(s => s);

            if (this.action != null)
            {
                this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.ActionType);

                this.TweetMessageTextBox.Text = this.action.TweetText;
                this.TweetImagePathTextBox.Text = this.action.ImagePath;

                this.NewProfileNameTextBox.Text = this.action.NewProfileName;
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (this.TypeComboBox.SelectedIndex >= 0)
            {
                TwitterActionTypeEnum type = EnumHelper.GetEnumValueFromString<TwitterActionTypeEnum>((string)this.TypeComboBox.SelectedItem);
                if (type == TwitterActionTypeEnum.SendTweet)
                {
                    if (!string.IsNullOrEmpty(this.TweetMessageTextBox.Text))
                    {
                        return new TwitterAction(this.TweetMessageTextBox.Text, this.TweetImagePathTextBox.Text);
                    }
                }
                else if (type == TwitterActionTypeEnum.UpdateName)
                {
                    if (!string.IsNullOrEmpty(this.NewProfileNameTextBox.Text))
                    {
                        return new TwitterAction(this.NewProfileNameTextBox.Text);
                    }
                }
            }
            return null;
        }

        private void TypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            this.SendTweetGrid.Visibility = Visibility.Collapsed;
            this.UpdateNameGrid.Visibility = Visibility.Collapsed;

            if (this.TypeComboBox.SelectedIndex >= 0)
            {
                TwitterActionTypeEnum type = EnumHelper.GetEnumValueFromString<TwitterActionTypeEnum>((string)this.TypeComboBox.SelectedItem);
                if (type == TwitterActionTypeEnum.SendTweet)
                {
                    this.SendTweetGrid.Visibility = Visibility.Visible;
                }
                else if (type == TwitterActionTypeEnum.UpdateName)
                {
                    this.UpdateNameGrid.Visibility = Visibility.Visible;
                }
            }
        }
    }
}
