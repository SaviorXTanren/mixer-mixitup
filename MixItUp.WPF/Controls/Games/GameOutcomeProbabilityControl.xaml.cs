using MixItUp.Base.Commands;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Games
{
    /// <summary>
    /// Interaction logic for GameOutcomeProbabilityControl.xaml
    /// </summary>
    public partial class GameOutcomeProbabilityControl : UserControl
    {
        public string OutcomeName { get; private set; }

        public GameOutcomeProbabilityControl(GameOutcomeProbability outcomeProbability)
        {
            InitializeComponent();

            this.ProbabilitySlider.Value = outcomeProbability.Probability;
            this.PayoutSlider.Value = outcomeProbability.Payout;
        }

        public GameOutcomeProbabilityControl()
        {
            InitializeComponent();

            this.ProbabilityTextBlock.Text = "0%";
            this.PayoutTextBlock.Text = "0%";
        }

        public GameOutcomeProbability GetOutcomeProbability()
        {
            return new GameOutcomeProbability((int)this.ProbabilitySlider.Value, (int)this.PayoutSlider.Value);
        }

        private void ProbabilitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.ProbabilityTextBlock.Text = ((int)this.ProbabilitySlider.Value).ToString() + "%";
        }

        private void PayoutSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.PayoutTextBlock.Text = ((int)this.PayoutSlider.Value).ToString() + "%";
        }
    }
}
