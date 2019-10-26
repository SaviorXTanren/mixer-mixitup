using MixItUp.Base.Actions;
using MixItUp.Base.Util;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for SpecialIdentifierActionControl.xaml
    /// </summary>
    public partial class SpecialIdentifierActionControl : ActionControlBase
    {
        private SpecialIdentifierAction action;

        public SpecialIdentifierActionControl() : base() { InitializeComponent(); }

        public SpecialIdentifierActionControl(SpecialIdentifierAction action) : this() { this.action = action; }

        public override Task OnLoaded()
        {
            if (this.action != null)
            {
                this.SpecialIdentifierNameTextBox.Text = this.action.SpecialIdentifierName;
                this.SpecialIdentifierReplacementTextBox.Text = this.action.SpecialIdentifierReplacement;
                this.MakeGloballyUsableCheckBox.IsChecked = this.action.MakeGloballyUsable;
                this.SpecialIdentifierShouldProcessMathCheckBox.IsChecked = this.action.SpecialIdentifierShouldProcessMath;
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (SpecialIdentifierStringBuilder.IsValidSpecialIdentifier(this.SpecialIdentifierNameTextBox.Text) && !string.IsNullOrEmpty(this.SpecialIdentifierReplacementTextBox.Text))
            {
                return new SpecialIdentifierAction(this.SpecialIdentifierNameTextBox.Text, this.SpecialIdentifierReplacementTextBox.Text,
                    this.MakeGloballyUsableCheckBox.IsChecked.GetValueOrDefault(), this.SpecialIdentifierShouldProcessMathCheckBox.IsChecked.GetValueOrDefault());
            }
            return null;
        }
    }
}
