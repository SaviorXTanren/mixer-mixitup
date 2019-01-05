using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Requirement
{
    /// <summary>
    /// Interaction logic for CurrencyRankInventoryRequirementControl.xaml
    /// </summary>
    public partial class CurrencyRankInventoryRequirementControl : UserControl
    {
        public CurrencyRankInventoryRequirementControl()
        {
            InitializeComponent();
        }

        public void HideCurrencyRequirement()
        {
            this.CurrencyRequirement.Visibility = Visibility.Collapsed;
        }
    }
}
