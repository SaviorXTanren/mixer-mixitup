using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Requirements
{
    /// <summary>
    /// Interaction logic for RequirementsSetControl.xaml
    /// </summary>
    public partial class RequirementsSetControl : GroupBox
    {
        public static readonly DependencyProperty ShowRoleProperty = DependencyProperty.Register("ShowRole", typeof(bool), typeof(RequirementsSetControl), new PropertyMetadata(true));
        public static readonly DependencyProperty ShowCooldownProperty = DependencyProperty.Register("ShowCooldown", typeof(bool), typeof(RequirementsSetControl), new PropertyMetadata(true));
        public static readonly DependencyProperty ShowCurrencyProperty = DependencyProperty.Register("ShowCurrency", typeof(bool), typeof(RequirementsSetControl), new PropertyMetadata(true));
        public static readonly DependencyProperty ShowRankProperty = DependencyProperty.Register("ShowRank", typeof(bool), typeof(RequirementsSetControl), new PropertyMetadata(true));
        public static readonly DependencyProperty ShowInventoryProperty = DependencyProperty.Register("ShowInventory", typeof(bool), typeof(RequirementsSetControl), new PropertyMetadata(true));
        public static readonly DependencyProperty ShowArgumentsProperty = DependencyProperty.Register("ShowArguments", typeof(bool), typeof(RequirementsSetControl), new PropertyMetadata(true));
        public static readonly DependencyProperty ShowThresholdProperty = DependencyProperty.Register("ShowThreshold", typeof(bool), typeof(RequirementsSetControl), new PropertyMetadata(true));
        public static readonly DependencyProperty ShowSettingsProperty = DependencyProperty.Register("ShowSettings", typeof(bool), typeof(RequirementsSetControl), new PropertyMetadata(true));

        public bool ShowRole
        {
            get { return (bool)GetValue(ShowRoleProperty); }
            set { SetValue(ShowRoleProperty, value); }
        }

        public bool ShowCooldown
        {
            get { return (bool)GetValue(ShowCooldownProperty); }
            set { SetValue(ShowCooldownProperty, value); }
        }

        public bool ShowCurrency
        {
            get { return (bool)GetValue(ShowCurrencyProperty); }
            set { SetValue(ShowCurrencyProperty, value); }
        }

        public bool ShowRank
        {
            get { return (bool)GetValue(ShowRankProperty); }
            set { SetValue(ShowRankProperty, value); }
        }

        public bool ShowInventory
        {
            get { return (bool)GetValue(ShowInventoryProperty); }
            set { SetValue(ShowInventoryProperty, value); }
        }

        public bool ShowArguments
        {
            get { return (bool)GetValue(ShowArgumentsProperty); }
            set { SetValue(ShowArgumentsProperty, value); }
        }

        public bool ShowThreshold
        {
            get { return (bool)GetValue(ShowThresholdProperty); }
            set { SetValue(ShowThresholdProperty, value); }
        }

        public bool ShowSettings
        {
            get { return (bool)GetValue(ShowSettingsProperty); }
            set { SetValue(ShowSettingsProperty, value); }
        }

        public RequirementsSetControl()
        {
            this.Loaded += RequirementsSetControl_Loaded;
            this.DataContextChanged += RequirementsSetControl_DataContextChanged;

            InitializeComponent();
        }

        private void RequirementsSetControl_Loaded(object sender, RoutedEventArgs e) { this.RefreshUI(); }

        private void RequirementsSetControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) { this.RefreshUI(); }

        private void RefreshUI()
        {
            if (this.Role != null) { this.Role.Visibility = this.ShowRole ? Visibility.Visible : Visibility.Collapsed; }
            if (this.Cooldown != null) { this.Cooldown.Visibility = this.ShowCooldown ? Visibility.Visible : Visibility.Collapsed; }
            if (this.Currency != null) { this.Currency.Visibility = this.ShowCurrency ? Visibility.Visible : Visibility.Collapsed; }
            if (this.Rank != null) { this.Rank.Visibility = this.ShowRank ? Visibility.Visible : Visibility.Collapsed; }
            if (this.Inventory != null) { this.Inventory.Visibility = this.ShowInventory ? Visibility.Visible : Visibility.Collapsed; }
            if (this.Arguments != null) { this.Arguments.Visibility = this.ShowArguments ? Visibility.Visible : Visibility.Collapsed; }
            if (this.Threshold != null) { this.Threshold.Visibility = this.ShowThreshold ? Visibility.Visible : Visibility.Collapsed; }
            if (this.Settings != null) { this.Settings.Visibility = this.ShowSettings ? Visibility.Visible : Visibility.Collapsed; }
        }
    }
}
