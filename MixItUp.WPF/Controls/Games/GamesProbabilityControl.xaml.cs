using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.WPF.Controls.Actions;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Games
{
    /// <summary>
    /// Interaction logic for GamesProbabilityControl.xaml
    /// </summary>
    public partial class GamesProbabilityControl : LoadingControlBase
    {
        private GameCommandWindow window;
        private GameResultProbability resultProbability;

        public GamesProbabilityControl(GameCommandWindow window)
        {
            this.window = window;

            InitializeComponent();
        }

        public GamesProbabilityControl(GameCommandWindow window, GameResultProbability resultProbability)
            : this(window)
        {
            this.ProbabilityAmountTextBox.Text = resultProbability.Probability.ToString();
            this.ProbabilityPayoutEquationTextBox.Text = resultProbability.PayoutEquation;
            this.resultProbability = resultProbability;
        }

        public async Task<bool> Validate()
        {
            double probability;
            if (!double.TryParse(this.ProbabilityAmountTextBox.Text, out probability) && probability >= 0.0 && probability <= 100.0)
            {
                await MessageBoxHelper.ShowMessageDialog("Result Probability must be a value decimal between 0 & 100");
                return false;
            }

            return true;
        }

        public GameResultProbability GetResultProbability()
        {
            double probability;
            if (double.TryParse(this.ProbabilityAmountTextBox.Text, out probability) && probability >= 0.0 && probability <= 100.0)
            {
                return new GameResultProbability(probability, this.ProbabilityPayoutEquationTextBox.Text, this.ProbabilityCommandControl.GetCommand());
            }
            return null;
        }

        protected override async Task OnLoaded()
        {
            await this.ProbabilityCommandControl.Initialize(this.window, "Command", (this.resultProbability != null) ? this.resultProbability.ResultCommand : null);
        }

        private async Task<List<ActionBase>> GetActions(IEnumerable<ActionContainerControl> containerControls)
        {
            List<ActionBase> actions = new List<ActionBase>();
            foreach (ActionContainerControl control in containerControls)
            {
                ActionBase action = control.GetAction();
                if (action == null)
                {
                    await MessageBoxHelper.ShowMessageDialog("Required action information is missing");
                    return new List<ActionBase>();
                }
                actions.Add(action);
            }
            return actions;
        }

        private void DeleteProbabilityButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.window.DeleteProbability(this);
        }
    }
}
