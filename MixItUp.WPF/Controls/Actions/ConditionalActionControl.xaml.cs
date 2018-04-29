using Mixer.Base.Util;
using MixItUp.Base.Actions;
using System.Threading.Tasks;

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
                this.Value2TextBox.Text = this.action.Value2;
                this.CommandReference.Command = this.action.GetCommand();
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (!string.IsNullOrEmpty(this.Value1TextBox.Text) && this.ComparisionTypeComboBox.SelectedIndex >= 0 && !string.IsNullOrEmpty(this.Value2TextBox.Text) &&
                this.CommandReference.Command != null)
            {
                return new ConditionalAction(EnumHelper.GetEnumValueFromString<ConditionalComparisionTypeEnum>((string)this.ComparisionTypeComboBox.SelectedItem),
                    this.IgnoreCasingToggleButton.IsChecked.GetValueOrDefault(), this.Value1TextBox.Text, this.Value2TextBox.Text, this.CommandReference.Command);
            }
            return null;
        }
    }
}
