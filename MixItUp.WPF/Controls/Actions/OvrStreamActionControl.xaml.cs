using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
        private bool shouldShowIntellisense = false;
        private ObservableCollection<VariablePair> variablePairs = new ObservableCollection<VariablePair>();
        private VariablePair activeVariablePair = null;
        private TextBox activeVariableNameTextBox = null;

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

            this.shouldShowIntellisense = false;
            if (this.action != null)
            {
                this.TypeComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.OvrStreamActionType);
                if (this.action.OvrStreamActionType == OvrStreamActionTypeEnum.UpdateVariables ||
                    this.action.OvrStreamActionType == OvrStreamActionTypeEnum.PlayTitle)
                {
                    this.TitleNameTextBox.Text = this.action.TitleName;
                    this.variablePairs.Clear();
                    foreach (var kvp in this.action.Variables)
                    {
                        this.variablePairs.Add(new VariablePair() { Name = kvp.Key, Value = kvp.Value });
                    }
                }
                else if (this.action.OvrStreamActionType == OvrStreamActionTypeEnum.HideTitle ||
                    this.action.OvrStreamActionType == OvrStreamActionTypeEnum.EnableTitle ||
                    this.action.OvrStreamActionType == OvrStreamActionTypeEnum.DisableTitle)
                {
                    this.TitleNameTextBox.Text = this.action.TitleName;
                }
            }
            this.shouldShowIntellisense = true;

            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (this.TypeComboBox.SelectedIndex >= 0)
            {
                OvrStreamActionTypeEnum actionType = EnumHelper.GetEnumValueFromString<OvrStreamActionTypeEnum>((string)this.TypeComboBox.SelectedItem);

                if (actionType == OvrStreamActionTypeEnum.UpdateVariables ||
                    actionType == OvrStreamActionTypeEnum.PlayTitle)
                {
                    // Must have a title name and cannot have duplicate variables names
                    if (!string.IsNullOrEmpty(this.TitleNameTextBox.Text) && this.variablePairs.Select(v => v.Name).Distinct().Count() == this.variablePairs.Count)
                    {
                        foreach (VariablePair pair in this.variablePairs)
                        {
                            // Don't allow empty variable names either
                            if (string.IsNullOrEmpty(pair.Name))
                            {
                                return null;
                            }
                        }
                        return OvrStreamAction.CreateVariableTitleAction(actionType, this.TitleNameTextBox.Text, this.variablePairs.ToDictionary(p => p.Name, p => p.Value));
                    }
                }
                else if (actionType == OvrStreamActionTypeEnum.HideTitle ||
                    actionType == OvrStreamActionTypeEnum.EnableTitle ||
                    actionType == OvrStreamActionTypeEnum.DisableTitle)
                {
                    if (!string.IsNullOrEmpty(this.TitleNameTextBox.Text))
                    {
                        return OvrStreamAction.CreateTitleAction(actionType, this.TitleNameTextBox.Text);
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
                if (actionType == OvrStreamActionTypeEnum.UpdateVariables ||
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

        private async void TitleNameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            HideTitleIntellisense();
            if (shouldShowIntellisense)
            {
                if (ChannelSession.Services.OvrStreamWebsocket != null && !string.IsNullOrEmpty(this.TitleNameTextBox.Text))
                {
                    var titles = (await ChannelSession.Services.OvrStreamWebsocket.GetTitles())
                        .OrderBy(t => t.Name)
                        .Where(t => t.Name.IndexOf(this.TitleNameTextBox.Text, StringComparison.InvariantCultureIgnoreCase) >= 0)
                        .Take(5)
                        .ToList();

                    if (titles.Count > 0)
                    {
                        TitleNameIntellisenseListBox.ItemsSource = titles;

                        // Select the first title
                        TitleNameIntellisenseListBox.SelectedIndex = 0;

                        Rect positionOfCarat = this.TitleNameTextBox.GetRectFromCharacterIndex(this.TitleNameTextBox.CaretIndex, true);
                        if (!positionOfCarat.IsEmpty)
                        {
                            Point topLeftOffset = this.TitleNameTextBox.TransformToAncestor(MainGrid).Transform(new Point(positionOfCarat.Left, positionOfCarat.Top));
                            ShowTitleIntellisense(topLeftOffset.X, topLeftOffset.Y);
                        }
                    }
                }
            }
        }

        private async void VariableNameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            HideVariableNameIntellisense();
            if (shouldShowIntellisense)
            {
                TextBox variableNameTextBox = sender as TextBox;
                if (ChannelSession.Services.OvrStreamWebsocket != null && !string.IsNullOrEmpty(variableNameTextBox.Text))
                {
                    var title = (await ChannelSession.Services.OvrStreamWebsocket.GetTitles())
                        .SingleOrDefault(t => t.Name.Equals(this.TitleNameTextBox.Text, StringComparison.InvariantCultureIgnoreCase));

                    if (title != null)
                    {
                        var variables = title.Variables
                            .OrderBy(v => v.Name)
                            .Where(v => v.Name.IndexOf(variableNameTextBox.Text, StringComparison.InvariantCultureIgnoreCase) >= 0)
                            .Take(5)
                            .ToList();

                        if (variables.Count > 0)
                        {
                            VariableNameIntellisenseListBox.ItemsSource = variables;
                            this.activeVariablePair = variableNameTextBox.DataContext as VariablePair;
                            this.activeVariableNameTextBox = variableNameTextBox;

                            // Select the first variable
                            VariableNameIntellisenseListBox.SelectedIndex = 0;

                            Rect positionOfCarat = variableNameTextBox.GetRectFromCharacterIndex(variableNameTextBox.CaretIndex, true);
                            if (!positionOfCarat.IsEmpty)
                            {
                                Point topLeftOffset = variableNameTextBox.TransformToAncestor(MainGrid).Transform(new Point(positionOfCarat.Left, positionOfCarat.Top));
                                ShowVariableNameIntellisense(topLeftOffset.X, topLeftOffset.Y);
                            }
                        }
                    }
                }
            }
        }

        private void TitleNameTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (this.TitleNameIntellisense.IsPopupOpen)
            {
                switch (e.Key)
                {
                    case Key.Escape:
                        HideTitleIntellisense();
                        e.Handled = true;
                        break;
                    case Key.Tab:
                    case Key.Enter:
                        SelectIntellisenseTitle();
                        e.Handled = true;
                        break;
                    case Key.Up:
                        TitleNameIntellisenseListBox.SelectedIndex = MathHelper.Clamp(TitleNameIntellisenseListBox.SelectedIndex - 1, 0, TitleNameIntellisenseListBox.Items.Count - 1);
                        e.Handled = true;
                        break;
                    case Key.Down:
                        TitleNameIntellisenseListBox.SelectedIndex = MathHelper.Clamp(TitleNameIntellisenseListBox.SelectedIndex + 1, 0, TitleNameIntellisenseListBox.Items.Count - 1);
                        e.Handled = true;
                        break;
                }
            }
        }

        private void VariableNameTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (this.VariableNameIntellisense.IsPopupOpen)
            {
                switch (e.Key)
                {
                    case Key.Escape:
                        HideVariableNameIntellisense();
                        e.Handled = true;
                        break;
                    case Key.Tab:
                    case Key.Enter:
                        SelectIntellisenseVariableName();
                        e.Handled = true;
                        break;
                    case Key.Up:
                        VariableNameIntellisenseListBox.SelectedIndex = MathHelper.Clamp(VariableNameIntellisenseListBox.SelectedIndex - 1, 0, VariableNameIntellisenseListBox.Items.Count - 1);
                        e.Handled = true;
                        break;
                    case Key.Down:
                        VariableNameIntellisenseListBox.SelectedIndex = MathHelper.Clamp(VariableNameIntellisenseListBox.SelectedIndex + 1, 0, VariableNameIntellisenseListBox.Items.Count - 1);
                        e.Handled = true;
                        break;
                }
            }
        }

        private void TitleNameIntellisenseListBox_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            SelectIntellisenseTitle();
        }

        private void VariableNameIntellisenseListBox_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            SelectIntellisenseVariableName();
        }

        private void ShowTitleIntellisense(double x, double y)
        {
            this.TitleNameIntellisenseContent.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(this.TitleNameIntellisense, x + 10);
            Canvas.SetTop(this.TitleNameIntellisense, y - 10);
            this.TitleNameIntellisense.UpdateLayout();

            if (!this.TitleNameIntellisense.IsPopupOpen)
            {
                this.TitleNameIntellisense.IsPopupOpen = true;
            }
        }

        private void ShowVariableNameIntellisense(double x, double y)
        {
            this.VariableNameIntellisenseContent.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(this.VariableNameIntellisense, x + 10);
            Canvas.SetTop(this.VariableNameIntellisense, y - 10);
            this.VariableNameIntellisense.UpdateLayout();

            if (!this.VariableNameIntellisense.IsPopupOpen)
            {
                this.VariableNameIntellisense.IsPopupOpen = true;
            }
        }

        private void HideTitleIntellisense()
        {
            if (this.TitleNameIntellisense.IsPopupOpen)
            {
                this.TitleNameIntellisense.IsPopupOpen = false;
            }
        }

        private void HideVariableNameIntellisense()
        {
            if (this.VariableNameIntellisense.IsPopupOpen)
            {
                this.VariableNameIntellisense.IsPopupOpen = false;
            }
        }

        private void SelectIntellisenseTitle()
        {
            this.shouldShowIntellisense = false;
            OvrStreamTitle title = TitleNameIntellisenseListBox.SelectedItem as OvrStreamTitle;
            if (title != null)
            {
                this.TitleNameTextBox.Text = title.Name;
                this.TitleNameTextBox.CaretIndex = this.TitleNameTextBox.Text.Length;
            }
            this.shouldShowIntellisense = true;

            HideTitleIntellisense();
        }

        private void SelectIntellisenseVariableName()
        {
            this.shouldShowIntellisense = false;
            if (this.activeVariablePair != null && this.activeVariableNameTextBox != null)
            {
                OvrStreamVariable variable = VariableNameIntellisenseListBox.SelectedItem as OvrStreamVariable;
                if (variable != null)
                {
                    this.activeVariablePair.Name = variable.Name;
                    this.activeVariableNameTextBox.Text = variable.Name;
                    this.activeVariableNameTextBox.CaretIndex = this.activeVariableNameTextBox.Text.Length;
                }
            }
            this.shouldShowIntellisense = true;

            HideVariableNameIntellisense();
        }
    }
}
