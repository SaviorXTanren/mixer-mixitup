using MixItUp.Base.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for CurrencyActionControl.xaml
    /// </summary>
    public partial class CurrencyActionControl : ActionControlBase
    {
        private CurrencyAction action;

        public CurrencyActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public CurrencyActionControl(ActionContainerControl containerControl, CurrencyAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            if (this.action != null)
            {
                this.CurrencyAmountTextBox.Text = this.action.Amount.ToString();
                this.CurrencyMessageTextBox.Text = this.action.ChatText;
                this.CurrencyWhisperToggleButton.IsChecked = this.action.IsWhisper;
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            int currencyAmount;
            if (!string.IsNullOrEmpty(this.CurrencyAmountTextBox.Text) && int.TryParse(this.CurrencyAmountTextBox.Text, out currencyAmount))
            {
                return new CurrencyAction(currencyAmount, this.CurrencyMessageTextBox.Text, this.CurrencyWhisperToggleButton.IsChecked.GetValueOrDefault());
            }
            return null;
        }
    }
}
