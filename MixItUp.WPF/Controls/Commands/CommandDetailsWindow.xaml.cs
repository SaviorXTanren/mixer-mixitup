using MixItUp.Base.Commands;
using System.Windows;

namespace MixItUp.WPF.Controls.Commands
{
    /// <summary>
    /// Interaction logic for CommandDetailsWindow.xaml
    /// </summary>
    public partial class CommandDetailsWindow : Window
    {
        private CommandBase command;

        public ChatCommand ChatCommand { get { return (ChatCommand)this.command; } }
        public InteractiveCommand InteractiveCommand { get { return (InteractiveCommand)this.command; } }
        public EventCommand EventCommand { get { return (EventCommand)this.command; } }

        public CommandDetailsWindow() : this(null) { }

        public CommandDetailsWindow(CommandBase command)
        {
            InitializeComponent();

            this.command = command;
        }
    }
}
