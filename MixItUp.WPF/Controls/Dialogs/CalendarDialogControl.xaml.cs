using System;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Dialogs
{
    /// <summary>
    /// Interaction logic for CalendarDialogControl.xaml
    /// </summary>
    public partial class CalendarDialogControl : UserControl
    {
        private DateTimeOffset initialDate;

        public CalendarDialogControl(DateTimeOffset date)
        {
            this.initialDate = date;

            InitializeComponent();

            this.Loaded += CalendarDialogControl_Loaded;
        }

        private void CalendarDialogControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Calendar.SelectedDate = this.initialDate.LocalDateTime;
        }

        public DateTimeOffset SelectedDate
        {
            get
            {
                if (this.Calendar.SelectedDate != null)
                {
                    return (DateTimeOffset)DateTime.SpecifyKind(this.Calendar.SelectedDate.GetValueOrDefault(), DateTimeKind.Local);
                }
                return this.initialDate;
            }
        }
    }
}
