using MixItUp.Base.Util;
using MixItUp.WPF.Controls.Dialogs;
using System;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls
{
    /// <summary>
    /// Interaction logic for ResetTrackerControl.xaml
    /// </summary>
    public partial class ResetTrackerControl : UserControl
    {
        public ResetTrackerControl()
        {
            InitializeComponent();
        }

        private async void DateButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext is ResetTrackerViewModel)
            {
                ResetTrackerViewModel viewModel = (ResetTrackerViewModel)this.DataContext;
                CalendarDialogControl calendarControl = new CalendarDialogControl(viewModel.StartDateTime);
                if (bool.Equals(await DialogHelper.ShowCustom(calendarControl), true))
                {
                    if (calendarControl.SelectedDate.Date >= DateTimeOffset.Now)
                    {
                        viewModel.StartDateTime = calendarControl.SelectedDate.Date;
                    }
                    else
                    {
                        await DialogHelper.ShowMessage(MixItUp.Base.Resources.ErrorDateCanNotBeInThePast);
                    }
                }
            }
        }
    }
}
