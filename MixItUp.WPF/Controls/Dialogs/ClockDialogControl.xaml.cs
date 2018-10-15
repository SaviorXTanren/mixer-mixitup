using System;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Dialogs
{
    /// <summary>
    /// Interaction logic for ClockDialogControl.xaml
    /// </summary>
    public partial class ClockDialogControl : UserControl
    {
        private DateTimeOffset initialTime;

        public ClockDialogControl(DateTimeOffset time)
        {
            this.initialTime = time;

            InitializeComponent();

            this.Loaded += CalendarDialogControl_Loaded;
        }

        private void CalendarDialogControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Clock.Time = this.initialTime.LocalDateTime;
        }

        public DateTimeOffset SelectedTime
        {
            get
            {
                return (DateTimeOffset)DateTime.SpecifyKind(this.Clock.Time, DateTimeKind.Local);
            }
        }
    }
}
