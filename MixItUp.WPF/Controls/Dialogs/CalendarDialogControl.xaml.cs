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
        private string header;

        public CalendarDialogControl(DateTimeOffset date) : this(date, null) { }

        public CalendarDialogControl(DateTimeOffset date, string header)
        {
            this.initialDate = date;
            this.header = header;

            InitializeComponent();

            this.Loaded += CalendarDialogControl_Loaded;
        }

        private void CalendarDialogControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Calendar.SelectedDate = this.initialDate.LocalDateTime;
            if (!string.IsNullOrEmpty(this.header))
            {
                this.HeaderTextBlock.Visibility = System.Windows.Visibility.Visible;
                this.HeaderTextBlock.Text = this.header;
            }
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
