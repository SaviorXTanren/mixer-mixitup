using MixItUp.Base.Actions;
using MixItUp.Base.Util;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for SpecialIdentifierActionControl.xaml
    /// </summary>
    public partial class SpecialIdentifierActionControl : ActionControlBase
    {
        private SpecialIdentifierAction action;

        public SpecialIdentifierActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public SpecialIdentifierActionControl(ActionContainerControl containerControl, SpecialIdentifierAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            if (this.action != null)
            {
                this.SpecialIdentifierNameTextBox.Text = this.action.SpecialIdentifierName;
                this.SpecialIdentifierReplacementTextBox.Text = this.action.SpecialIdentifierReplacement;
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (SpecialIdentifierStringBuilder.IsValidSpecialIdentifier(this.SpecialIdentifierNameTextBox.Text) && !string.IsNullOrEmpty(this.SpecialIdentifierReplacementTextBox.Text))
            {
                return new SpecialIdentifierAction(this.SpecialIdentifierNameTextBox.Text, this.SpecialIdentifierReplacementTextBox.Text);
            }
            return null;
        }
    }
}
