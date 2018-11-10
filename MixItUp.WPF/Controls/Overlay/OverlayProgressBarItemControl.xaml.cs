using Mixer.Base.Util;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Themes;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayProgressBarItemControl.xaml
    /// </summary>
    public partial class OverlayProgressBarItemControl : OverlayItemControl
    {
        private OverlayProgressBarItem item;

        public OverlayProgressBarItemControl()
        {
            InitializeComponent();
        }

        public OverlayProgressBarItemControl(OverlayProgressBarItem item)
            : this()
        {
            this.item = item;
        }

        public override void SetItem(OverlayItemBase item)
        {
            this.item = (OverlayProgressBarItem)item;
            this.GoalTypeComboBox.SelectedItem = EnumHelper.GetEnumName(this.item.ProgressBarType);

            this.StartingAmountTextBox.IsEnabled = true;
            this.GoalAmountTextBox.IsEnabled = true;
            this.ResetAfterDaysTextBox.IsEnabled = true;

            if (this.item.ProgressBarType == ProgressBarTypeEnum.Milestones)
            {
                this.StartingAmountTextBox.Text = string.Empty;
                this.StartingAmountTextBox.IsEnabled = false;
                this.GoalAmountTextBox.Text = string.Empty;
                this.GoalAmountTextBox.IsEnabled = false;
                this.ResetAfterDaysTextBox.Text = string.Empty;
                this.ResetAfterDaysTextBox.IsEnabled = false;
            }
            else
            {
                if (this.item.ProgressBarType == ProgressBarTypeEnum.Followers && this.item.CurrentAmountNumber < 0)
                {
                    this.TotalFollowsCheckBox.IsChecked = true;
                    this.StartingAmountTextBox.Text = "0";
                }
                else
                {
                    if (!string.IsNullOrEmpty(this.item.CurrentAmountCustom))
                    {
                        this.StartingAmountTextBox.Text = this.item.CurrentAmountCustom.ToString();
                    }
                    else
                    {
                        this.StartingAmountTextBox.Text = this.item.CurrentAmountNumber.ToString();
                    }
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
            }

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

            this.WidthTextBox.Text = this.item.Width.ToString();
            this.HeightTextBox.Text = this.item.Height.ToString();

            this.HTMLText.Text = this.item.HTMLText;
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
                    return new OverlayProgressBarItem(this.HTMLText.Text, type, startingAmountNumber, goalAmountNumber, resetAfterDays, progressColor, backgroundColor, textColor, width, height);
                }
                else
                {
                    return new OverlayProgressBarItem(this.HTMLText.Text, type, startingAmount, goalAmount, resetAfterDays, progressColor, backgroundColor, textColor, width, height);
                }
            }
            return null;
        }

        protected override Task OnLoaded()
        {
            this.GoalTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<ProgressBarTypeEnum>();
            this.ProgressColorComboBox.ItemsSource = this.BackgroundColorComboBox.ItemsSource = this.TextColorComboBox.ItemsSource = ColorSchemes.ColorSchemeDictionary.Keys;

            this.StartingAmountTextBox.Text = "0";
            this.GoalAmountTextBox.Text = "0";
            this.ResetAfterDaysTextBox.Text = "0";
            this.HTMLText.Text = OverlayProgressBarItem.HTMLTemplate;

            if (this.item != null)
            {
                this.SetItem(this.item);
            }

            return Task.FromResult(0);
        }

        private void GoalTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.GoalTypeComboBox.SelectedIndex >= 0)
            {
                ProgressBarTypeEnum type = EnumHelper.GetEnumValueFromString<ProgressBarTypeEnum>((string)this.GoalTypeComboBox.SelectedItem);
                if (type == ProgressBarTypeEnum.Followers)
                {
                    this.TotalFollowsCheckBox.IsChecked = true;
                    this.TotalFollowsGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    this.TotalFollowsCheckBox.IsChecked = false;
                    this.TotalFollowsGrid.Visibility = Visibility.Collapsed;
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
    }
}
