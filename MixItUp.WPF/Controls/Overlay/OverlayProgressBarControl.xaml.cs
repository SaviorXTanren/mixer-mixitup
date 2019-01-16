﻿using Mixer.Base.Util;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayProgressBarControl.xaml
    /// </summary>
    public partial class OverlayProgressBarControl : OverlayItemControl
    {
        private OverlayProgressBar item;

        private CustomCommand command;

        public OverlayProgressBarControl()
        {
            InitializeComponent();
        }

        public OverlayProgressBarControl(OverlayProgressBar item)
            : this()
        {
            this.item = item;
        }

        public override void SetItem(OverlayItemBase item)
        {
            this.item = (OverlayProgressBar)item;

            this.GoalTypeComboBox.SelectedItem = EnumHelper.GetEnumName(this.item.ProgressBarType);

            if (!string.IsNullOrEmpty(this.item.CurrentAmountCustom))
            {
                this.StartingAmountTextBox.Text = this.item.CurrentAmountCustom.ToString();
            }
            else
            {
                this.StartingAmountTextBox.Text = this.item.CurrentAmountNumber.ToString();
            }

            if (this.item.ProgressBarType == ProgressBarTypeEnum.Followers && this.item.CurrentAmountNumber < 0)
            {
                this.TotalFollowsCheckBox.IsChecked = true;
                this.StartingAmountTextBox.Text = "0";
            }

            if (!string.IsNullOrEmpty(this.item.GoalAmountCustom))
            {
                this.GoalAmountTextBox.Text = this.item.GoalAmountCustom.ToString();
            }
            else
            {
                this.GoalAmountTextBox.Text = this.item.GoalAmountNumber.ToString();
            }

            this.ResetAfterDaysTextBox.Text = this.item.ResetAfterDays.ToString();

            this.ProgressColorComboBox.Text = this.item.ProgressColor;
            if (ColorSchemes.ColorSchemeDictionary.ContainsValue(this.item.ProgressColor))
            {
                this.ProgressColorComboBox.Text = ColorSchemes.ColorSchemeDictionary.FirstOrDefault(c => c.Value.Equals(this.item.ProgressColor)).Key;
            }

            this.BackgroundColorComboBox.Text = this.item.BackgroundColor;
            if (ColorSchemes.ColorSchemeDictionary.ContainsValue(this.item.BackgroundColor))
            {
                this.BackgroundColorComboBox.Text = ColorSchemes.ColorSchemeDictionary.FirstOrDefault(c => c.Value.Equals(this.item.BackgroundColor)).Key;
            }

            this.TextColorComboBox.Text = this.item.TextColor;
            if (ColorSchemes.ColorSchemeDictionary.ContainsValue(this.item.TextColor))
            {
                this.TextColorComboBox.Text = ColorSchemes.ColorSchemeDictionary.FirstOrDefault(c => c.Value.Equals(this.item.TextColor)).Key;
            }

            this.TextFontComboBox.Text = this.item.TextFont;

            this.WidthTextBox.Text = this.item.Width.ToString();
            this.HeightTextBox.Text = this.item.Height.ToString();

            this.HTMLText.Text = this.item.HTMLText;

            this.command = this.item.GoalReachedCommand;
            this.UpdateChangedCommand();
        }

        public override OverlayItemBase GetItem()
        {
            if (this.GoalTypeComboBox.SelectedIndex >= 0 && !string.IsNullOrEmpty(this.StartingAmountTextBox.Text) && !string.IsNullOrEmpty(this.GoalAmountTextBox.Text))
            {
                ProgressBarTypeEnum type = EnumHelper.GetEnumValueFromString<ProgressBarTypeEnum>((string)this.GoalTypeComboBox.SelectedItem);

                string startingAmount = "0";
                string goalAmount = "0";
                int resetAfterDays = 0;
                if (type != ProgressBarTypeEnum.Milestones)
                {
                    if (!(type == ProgressBarTypeEnum.Followers && this.TotalFollowsCheckBox.IsChecked.GetValueOrDefault()))
                    {
                        startingAmount = this.StartingAmountTextBox.Text;
                    }

                    goalAmount = this.GoalAmountTextBox.Text;

                    if (!string.IsNullOrEmpty(this.ResetAfterDaysTextBox.Text))
                    {
                        if (!int.TryParse(this.ResetAfterDaysTextBox.Text, out resetAfterDays))
                        {
                            return null;
                        }
                    }
                }

                string progressColor = this.ProgressColorComboBox.Text;
                if (ColorSchemes.ColorSchemeDictionary.ContainsKey(progressColor))
                {
                    progressColor = ColorSchemes.ColorSchemeDictionary[progressColor];
                }

                string backgroundColor = this.BackgroundColorComboBox.Text;
                if (ColorSchemes.ColorSchemeDictionary.ContainsKey(backgroundColor))
                {
                    backgroundColor = ColorSchemes.ColorSchemeDictionary[backgroundColor];
                }

                string textColor = this.TextColorComboBox.Text;
                if (ColorSchemes.ColorSchemeDictionary.ContainsKey(textColor))
                {
                    textColor = ColorSchemes.ColorSchemeDictionary[textColor];
                }

                if (string.IsNullOrEmpty(this.TextFontComboBox.Text))
                {
                    return null;
                }

                if (string.IsNullOrEmpty(this.WidthTextBox.Text) || !int.TryParse(this.WidthTextBox.Text, out int width) ||
                    string.IsNullOrEmpty(this.HeightTextBox.Text) || !int.TryParse(this.HeightTextBox.Text, out int height))
                {
                    return null;
                }

                if (string.IsNullOrEmpty(this.HTMLText.Text))
                {
                    return null;
                }

                if (double.TryParse(startingAmount, out double startingAmountNumber) && double.TryParse(goalAmount, out double goalAmountNumber))
                {
                    return new OverlayProgressBar(this.HTMLText.Text, type, startingAmountNumber, goalAmountNumber, resetAfterDays, progressColor, backgroundColor, textColor, this.TextFontComboBox.Text, width, height, this.command);
                }
                else
                {
                    return new OverlayProgressBar(this.HTMLText.Text, type, startingAmount, goalAmount, resetAfterDays, progressColor, backgroundColor, textColor, this.TextFontComboBox.Text, width, height, this.command);
                }
            }
            return null;
        }

        protected override Task OnLoaded()
        {
            this.GoalTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<ProgressBarTypeEnum>().OrderBy(s => s);
            this.ProgressColorComboBox.ItemsSource = this.BackgroundColorComboBox.ItemsSource = this.TextColorComboBox.ItemsSource = ColorSchemes.ColorSchemeDictionary.Keys;

            this.TextFontComboBox.ItemsSource = InstalledFonts.GetInstalledFonts();

            this.StartingAmountTextBox.Text = "0";
            this.GoalAmountTextBox.Text = "0";
            this.ResetAfterDaysTextBox.Text = "0";
            this.TextFontComboBox.Text = "Arial";
            this.HTMLText.Text = OverlayProgressBar.HTMLTemplate;

            if (this.item != null)
            {
                this.SetItem(this.item);
            }

            return Task.FromResult(0);
        }

        private void GoalTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            this.StartingAmountTextBox.IsEnabled = true;
            this.GoalAmountTextBox.IsEnabled = true;
            this.ResetAfterDaysTextBox.IsEnabled = true;

            this.TotalFollowsGrid.Visibility = Visibility.Collapsed;
            this.TotalFollowsCheckBox.IsChecked = false;

            if (this.GoalTypeComboBox.SelectedIndex >= 0)
            {
                ProgressBarTypeEnum type = EnumHelper.GetEnumValueFromString<ProgressBarTypeEnum>((string)this.GoalTypeComboBox.SelectedItem);
                if (type == ProgressBarTypeEnum.Milestones)
                {
                    this.StartingAmountTextBox.IsEnabled = false;
                    this.GoalAmountTextBox.IsEnabled = false;
                    this.ResetAfterDaysTextBox.IsEnabled = false;
                }
                else if (type == ProgressBarTypeEnum.Followers)
                {
                    this.TotalFollowsCheckBox.IsChecked = true;
                    this.TotalFollowsGrid.Visibility = Visibility.Visible;
                }
            }
        }

        private void TotalFollowsCheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            this.StartingAmountTextBox.IsEnabled = !this.TotalFollowsCheckBox.IsChecked.GetValueOrDefault();
            if (this.TotalFollowsCheckBox.IsChecked.GetValueOrDefault())
            {
                this.StartingAmountTextBox.Text = "0";
            }
        }

        private void NewCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(new CustomCommand(OverlayProgressBar.GoalReachedCommandName)));
            window.CommandSaveSuccessfully += Window_CommandSaveSuccessfully;
            window.Closed += Window_Closed;
            window.Show();
        }

        private void CommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            CustomCommand command = commandButtonsControl.GetCommandFromCommandButtons<CustomCommand>(sender);
            if (command != null)
            {
                CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(command));
                window.Closed += Window_Closed;
                window.Show();
            }
        }

        private void CommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            CustomCommand command = commandButtonsControl.GetCommandFromCommandButtons<CustomCommand>(sender);
            if (command != null)
            {
                this.command = null;
                this.UpdateChangedCommand();
            }
        }

        private void Window_CommandSaveSuccessfully(object sender, CommandBase e)
        {
            this.command = (CustomCommand)e;
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            this.UpdateChangedCommand();
        }

        private void UpdateChangedCommand()
        {
            if (this.command != null)
            {
                this.NewCommandButton.Visibility = Visibility.Collapsed;
                this.CommandButtons.Visibility = Visibility.Visible;
                this.CommandButtons.DataContext = this.command;
            }
            else
            {
                this.NewCommandButton.Visibility = Visibility.Visible;
                this.CommandButtons.Visibility = Visibility.Collapsed;
            }
        }
    }
}
