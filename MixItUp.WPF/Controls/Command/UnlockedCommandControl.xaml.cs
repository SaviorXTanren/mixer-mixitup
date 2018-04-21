using System;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Command
{
    /// <summary>
    /// Interaction logic for UnlockedCommandControl.xaml
    /// </summary>
    public partial class UnlockedCommandControl : UserControl
    {
        public static readonly string UnlockedGridTooltip =
            "Mix It Up has build in Command locking functionality which ensures only" + Environment.NewLine +
            "1 command type (Chat, Interactive, etc) can run at the same time and" + Environment.NewLine +
            "ensures that each command finishes in the order it was run in." + Environment.NewLine + Environment.NewLine +
            "This option will allow you to disable locking on this command, but be aware" + Environment.NewLine +
            "that this could cause some unforeseen issues so please use with caution.";

        public UnlockedCommandControl()
        {
            InitializeComponent();

            this.Loaded += UnlockedCommandControl_Loaded;
        }

        private void UnlockedCommandControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.UnlockedGrid.ToolTip = UnlockedGridTooltip;
        }

        public bool Unlocked
        {
            get { return this.UnlockCommandToggleButton.IsChecked.GetValueOrDefault(); }
            set { this.UnlockCommandToggleButton.IsChecked = value; }
        }
    }
}
