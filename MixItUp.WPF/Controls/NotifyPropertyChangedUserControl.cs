using System.ComponentModel;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls
{
    public class NotifyPropertyChangedUserControl : UserControl
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
