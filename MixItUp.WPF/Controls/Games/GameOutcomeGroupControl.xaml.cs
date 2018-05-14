using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Games
{
    /// <summary>
    /// Interaction logic for GameOutcomeGroupControl.xaml
    /// </summary>
    public partial class GameOutcomeGroupControl : UserControl
    {
        private ObservableCollection<GameOutcomeProbabilityControl> outcomeProbabilityControls = new ObservableCollection<GameOutcomeProbabilityControl>();

        public GameOutcomeGroupControl(GameOutcomeGroup outcomeGroup)
        {
            InitializeComponent();

            this.ProbabilityControlsItemsControl.ItemsSource = this.outcomeProbabilityControls;

            if (outcomeGroup.RankRequirement != null && outcomeGroup.RankRequirement.GetCurrency() != null)
            {
                this.RankGroupGrid.Visibility = Visibility.Visible;

                UserCurrencyViewModel rankCurrency = outcomeGroup.RankRequirement.GetCurrency();
                this.RankTypeComboBox.SelectedItem = rankCurrency;
                this.RankMinimumComboBox.SelectedItem = outcomeGroup.RankRequirement.RequiredRank;
            }
            else
            {
                this.PreDefinedGroupGrid.Visibility = Visibility.Visible;
                this.PreDefinedGroupNameTextBlock.Text = EnumHelper.GetEnumName(outcomeGroup.Role);
            }

            foreach (GameOutcomeProbability probability in outcomeGroup.Probabilities)
            {
                this.AddProbability(new GameOutcomeProbabilityControl(probability));
            }
        }

        public GameOutcomeGroupControl()
        {
            InitializeComponent();

            this.ProbabilityControlsItemsControl.ItemsSource = this.outcomeProbabilityControls;

            this.RankGroupGrid.Visibility = Visibility.Visible;
            IEnumerable<UserCurrencyViewModel> ranks = ChannelSession.Settings.Currencies.Values.Where(c => c.IsRank);
            if (ranks.Count() > 0)
            {
                this.RankTypeComboBox.ItemsSource = ChannelSession.Settings.Currencies.Values.Where(c => c.IsRank);
            }
        }

        public async Task<GameOutcomeGroup> GetOutcomeGroup()
        {
            GameOutcomeGroup group = null;
            string groupName = null;
            if (this.RankGroupGrid.Visibility != Visibility.Visible)
            {
                MixerRoleEnum role = EnumHelper.GetEnumValueFromString<MixerRoleEnum>((string)this.PreDefinedGroupNameTextBlock.Text);
                groupName = EnumHelper.GetEnumName(role);
                group = new GameOutcomeGroup(role);
            }
            else if (this.RankTypeComboBox.SelectedIndex >= 0 && this.RankMinimumComboBox.SelectedIndex >= 0)
            {
                CurrencyRequirementViewModel rankRequirement = new CurrencyRequirementViewModel((UserCurrencyViewModel)this.RankTypeComboBox.SelectedItem, (UserRankViewModel)this.RankMinimumComboBox.SelectedItem);
                groupName = rankRequirement.GetCurrency().Name + " - " + rankRequirement.RankName;
                group = new GameOutcomeGroup(rankRequirement);
            }
            else
            {
                await MessageBoxHelper.ShowMessageDialog("A group is missing the Rank Type & Rank Name");
                return null;
            }

            if (group != null && !string.IsNullOrEmpty(groupName))
            {
                foreach (GameOutcomeProbabilityControl probabilityControl in this.outcomeProbabilityControls)
                {
                    group.Probabilities.Add(probabilityControl.GetOutcomeProbability());
                }

                if (group.Probabilities.Sum(p => p.Probability) > 100)
                {
                    await MessageBoxHelper.ShowMessageDialog("The group " + groupName + " can not have a combined probability of more than 100%");
                    return null;
                }
            }

            return group;
        }

        public void AddProbability(GameOutcomeProbabilityControl control)
        {
            this.outcomeProbabilityControls.Add(control);           
        }

        public void DeleteProbability(GameOutcome outcome)
        {
            GameOutcomeProbabilityControl control = this.outcomeProbabilityControls.FirstOrDefault(opc => opc.OutcomeName.Equals(outcome.Name));
            if (control != null)
            {
                this.outcomeProbabilityControls.Remove(control);
            }
        }

        private void RankTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.RankTypeComboBox.SelectedIndex >= 0)
            {
                this.RankMinimumComboBox.IsEnabled = true;

                UserCurrencyViewModel rankCurrency = (UserCurrencyViewModel)this.RankTypeComboBox.SelectedItem;
                this.RankMinimumComboBox.ItemsSource = rankCurrency.Ranks;
            }
        }
    }
}
