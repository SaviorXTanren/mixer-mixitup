using Mixer.Base.Model.Interactive;
using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Actions
{
    public class VariablePair
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    /// <summary>
    /// Interaction logic for OvrStreamActionControl.xaml
    /// </summary>
    public partial class OvrStreamActionControl : ActionControlBase
    {
        private OvrStreamAction action;

        private ObservableCollection<VariablePair> variablePairs = new ObservableCollection<VariablePair>();

        public OvrStreamActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public OvrStreamActionControl(ActionContainerControl containerControl, OvrStreamAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            if (ChannelSession.Services.OvrStreamWebsocket == null)
            {
                this.OvrStreamNotEnabledWarningTextBlock.Visibility = Visibility.Visible;
            }

            this.TypeComboBox.ItemsSource = EnumHelper.GetEnumNames<OvrStreamActionTypeEnum>().OrderBy(s => s);

            this.VariableItemsControl.ItemsSource = this.variablePairs;

            if (this.action != null)
            {
                this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.OvrStreamActionType);
                if (this.action.OvrStreamActionType == OvrStreamActionTypeEnum.ShowTitle ||
                    this.action.OvrStreamActionType == OvrStreamActionTypeEnum.HideTitle ||
                    this.action.OvrStreamActionType == OvrStreamActionTypeEnum.PlayTitle)
                {
                    this.TitleNameTextBox.Text = this.action.TitleName;
                    this.variablePairs.Clear();
                    foreach (var kvp in this.action.Variables)
                    {
                        this.variablePairs.Add(new VariablePair() { Name = kvp.Key, Value = kvp.Value });
                    }
                }
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (this.TypeComboBox.SelectedIndex >= 0)
            {
                OvrStreamActionTypeEnum actionType = EnumHelper.GetEnumValueFromString<OvrStreamActionTypeEnum>((string)this.TypeComboBox.SelectedItem);

                if (actionType == OvrStreamActionTypeEnum.ShowTitle ||
                    actionType == OvrStreamActionTypeEnum.HideTitle ||
                    actionType == OvrStreamActionTypeEnum.PlayTitle)
                {
                    // Must have a title name and cannot have duplicate variables
                    if (!string.IsNullOrEmpty(this.TitleNameTextBox.Text) && this.variablePairs.Select(v => v.Name).Distinct().Count() == this.variablePairs.Count)
                    {
                        foreach (VariablePair pair in this.variablePairs)
                        {
                            if (string.IsNullOrEmpty(pair.Name) || string.IsNullOrEmpty(pair.Value))
                            {
                                return null;
                            }
                        }
                        return OvrStreamAction.CreateTriggerTitleAction(actionType, this.TitleNameTextBox.Text, this.variablePairs.ToDictionary(p => p.Name, p => p.Value));
                    }
                }
            }
            return null;
        }

        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.SetVariableGrid.Visibility = Visibility.Collapsed;
            if (this.TypeComboBox.SelectedIndex >= 0)
            {
                OvrStreamActionTypeEnum actionType = EnumHelper.GetEnumValueFromString<OvrStreamActionTypeEnum>((string)this.TypeComboBox.SelectedItem);
                if (actionType == OvrStreamActionTypeEnum.ShowTitle ||
                    actionType == OvrStreamActionTypeEnum.HideTitle ||
                    actionType == OvrStreamActionTypeEnum.PlayTitle)
                {
                    this.SetVariableGrid.Visibility = Visibility.Visible;
                }
            }
        }

        private void AddVariableButton_Click(object sender, RoutedEventArgs e)
        {
            this.variablePairs.Add(new VariablePair());
        }

        private void DeleteVariableButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            VariablePair pair = (VariablePair)button.DataContext;
            this.variablePairs.Remove(pair);
        }
    }
}
