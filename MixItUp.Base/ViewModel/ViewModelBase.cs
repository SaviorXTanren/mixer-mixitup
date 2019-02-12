using MixItUp.Base.Util;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModels
{
    public class ViewModelCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private Func<object, bool> canExecute;
        private Func<object, Task> execute;

        public ViewModelCommand(Func<object, Task> execute)
        {
            this.execute = execute;
        }

        public ViewModelCommand(Func<object, bool> canExecute, Func<object, Task> execute)
            : this(execute)
        {
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            try
            {
                if (this.canExecute != null)
                {
                    return this.canExecute(parameter);
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return false;
        }

        public async void Execute(object parameter)
        {
            try
            {
                await this.execute(parameter);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public void NotifyCanExecuteChanged() { this.CanExecuteChanged.Invoke(this, new EventArgs()); }
    }

    public class ModelViewModelBase<T> : ViewModelBase, IEquatable<ModelViewModelBase<T>>
    {
        protected T model;

        public ModelViewModelBase(T model)
        {
            this.model = model;
        }

        public T GetModel() { return this.model; }

        public override bool Equals(object other)
        {
            if (other is ModelViewModelBase<T>)
            {
                this.Equals((ModelViewModelBase<T>)other);
            }
            return false;
        }

        public bool Equals(ModelViewModelBase<T> other) { return this.model.Equals(other.GetModel()); }

        public override int GetHashCode() { return this.model.GetHashCode(); }
    }

    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected ICommand CreateCommand(Func<object, Task> execute) { return new ViewModelCommand(execute); }

        protected ICommand CreateCommand(Func<object, bool> canExecute, Func<object, Task> execute) { return new ViewModelCommand(canExecute, execute); }

        protected void NotifyPropertyChanged([CallerMemberName]string name = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
