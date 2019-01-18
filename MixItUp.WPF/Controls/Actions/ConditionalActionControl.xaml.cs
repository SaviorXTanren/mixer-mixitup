using Mixer.Base.Util;
using MixItUp.Base.Actions;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for ConditionalActionControl.xaml
    /// </summary>
    public partial class ConditionalActionControl : ActionControlBase
    {
        private ConditionalAction action;

        public ConditionalActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public ConditionalActionControl(ActionContainerControl containerControl, ConditionalAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            this.ComparisionTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<ConditionalComparisionTypeEnum>();
            if (this.action != null)
            {
                this.IgnoreCasingToggleButton.IsChecked = this.action.IgnoreCase;
                this.Value1TextBox.Text = this.action.Value1;
                this.ComparisionTypeComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.ComparisionType);
                if (this.action.ComparisionType == ConditionalComparisionTypeEnum.Between)
                {
                    this.Value2TextBox.Text = this.action.Value2;
                }
                else
                {
                    this.MinValue2TextBox.Text = this.action.Value2;
                    this.MaxValue3TextBox.Text = this.action.Value3;
                }
                this.CommandReference.Command = this.action.GetCommand();
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (!string.IsNullOrEmpty(this.Value1TextBox.Text) && this.ComparisionTypeComboBox.SelectedIndex >= 0 && this.CommandReference.Command != null)
            {
                ConditionalComparisionTypeEnum type = EnumHelper.GetEnumValueFromString<ConditionalComparisionTypeEnum>((string)this.ComparisionTypeComboBox.SelectedItem);
                if (type == ConditionalComparisionTypeEnum.Between)
                {
                    if (!string.IsNullOrEmpty(this.MinValue2TextBox.Text) && !string.IsNullOrEmpty(this.MaxValue3TextBox.Text))
                    {
                        return new ConditionalAction(type, this.IgnoreCasingToggleButton.IsChecked.GetValueOrDefault(), this.Value1TextBox.Text, this.MinValue2TextBox.Text,
                            this.MaxValue3TextBox.Text, this.CommandReference.Command);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(this.Value2TextBox.Text))
                    {
                        return new ConditionalAction(type, this.IgnoreCasingToggleButton.IsChecked.GetValueOrDefault(), this.Value1TextBox.Text, this.Value2TextBox.Text,
                            this.CommandReference.Command);
                    }
                }
            }
            return null;
        }

        private void ComparisionTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.ComparisionTypeComboBox.SelectedIndex >= 0)
            {
                ConditionalComparisionTypeEnum type = EnumHelper.GetEnumValueFromString<ConditionalComparisionTypeEnum>((string)this.ComparisionTypeComboBox.SelectedItem);
                if (type == ConditionalComparisionTypeEnum.Between)
                {
                    this.BetweenValuesGrid.Visibility = Visibility.Visible;
                    this.Value2TextBox.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.BetweenValuesGrid.Visibility = Visibility.Collapsed;
                    this.Value2TextBox.Visibility = Visibility.Visible;
                }
            }
        }
    }
}
