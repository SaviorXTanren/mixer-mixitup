using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MixItUp.Base.ViewModels
{
    public class NotifyPropertyChangedViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName]string name = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
