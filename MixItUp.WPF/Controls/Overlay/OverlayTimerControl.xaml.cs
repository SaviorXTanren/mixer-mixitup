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
    /// Interaction logic for OverlayTimerControl.xaml
    /// </summary>
    public partial class OverlayTimerControl : OverlayItemControl
    {
        private OverlayTimer item;

        private CustomCommand command;

        public OverlayTimerControl()
        {
            InitializeComponent();
        }

        public OverlayTimerControl(OverlayTimer item)
            : this()
        {
            this.item = item;
        }

        public override void SetItem(OverlayItemBase item)
        {
            this.item = (OverlayTimer)item;

            this.TotalLengthTextBox.Text = this.item.TotalLength.ToString();

            this.TextColorComboBox.Text = this.item.TextColor;
            if (ColorSchemes.ColorSchemeDictionary.ContainsValue(this.item.TextColor))
            {
                this.TextColorComboBox.Text = ColorSchemes.ColorSchemeDictionary.FirstOrDefault(c => c.Value.Equals(this.item.TextColor)).Key;
            }

            this.TextFontComboBox.Text = this.item.TextFont;

            this.TextSizeComboBox.Text = this.item.TextSize.ToString();

            this.HTMLText.Text = this.item.HTMLText;

            this.command = this.item.TimerCompleteCommand;
            this.UpdateChangedCommand();
        }

        public override OverlayItemBase GetItem()
        {
            if (!int.TryParse(this.TotalLengthTextBox.Text, out int length) || length <= 0)
            {
                return null;
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

            if (!int.TryParse(this.TextSizeComboBox.Text, out int size) || size <= 0)
            {
                return null;
            }

            if (string.IsNullOrEmpty(this.HTMLText.Text))
            {
                return null;
            }

            return new OverlayTimer(this.HTMLText.Text, length, textColor, this.TextFontComboBox.Text, size, this.command);
        }

        protected override Task OnLoaded()
        {
            this.TextColorComboBox.ItemsSource = ColorSchemes.ColorSchemeDictionary.Keys;
            this.TextFontComboBox.ItemsSource = InstalledFonts.GetInstalledFonts();
            this.TextSizeComboBox.ItemsSource = OverlayTextItemControl.sampleFontSize.Select(f => f.ToString());

            this.TextFontComboBox.Text = "Arial";
            this.HTMLText.Text = OverlayTimer.HTMLTemplate;

            if (this.item != null)
            {
                this.SetItem(this.item);
            }

            return Task.FromResult(0);
        }

        private void NewCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandWindow window = new CommandWindow(new CustomCommandDetailsControl(new CustomCommand(OverlayTimer.TimerCompleteCommandName)));
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
