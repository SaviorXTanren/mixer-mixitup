using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Requirement
{
    /// <summary>
    /// Interaction logic for CurrencyRankRequirementControl.xaml
    /// </summary>
    public partial class CurrencyRankRequirementControl : UserControl
    {
        public CurrencyRankRequirementControl()
        {
            InitializeComponent();
        }

        public void HideCurrencyRequirement()
        {
            this.CurrencyRequirement.Visibility = Visibility.Collapsed;
        }
    }
}
