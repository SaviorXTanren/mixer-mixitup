using System;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls
{
    public class SortedDataGrid : DataGrid
    {
        public event EventHandler<DataGridColumn> Sorted;

        protected override void OnSorting(DataGridSortingEventArgs eventArgs)
        {
            base.OnSorting(eventArgs);
            this.Sorted?.Invoke(this, eventArgs.Column);
        }
    }
}
