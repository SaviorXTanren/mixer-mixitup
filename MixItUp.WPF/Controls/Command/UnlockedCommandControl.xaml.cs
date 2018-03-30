using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Command
{
    /// <summary>
    /// Interaction logic for UnlockedCommandControl.xaml
    /// </summary>
    public partial class UnlockedCommandControl : UserControl
    {
        public UnlockedCommandControl()
        {
            InitializeComponent();
        }

        public bool Unlocked
        {
            get { return this.DontBlockCommandToggleButton.IsChecked.GetValueOrDefault(); }
            set { this.DontBlockCommandToggleButton.IsChecked = value; }
        }
    }
}
