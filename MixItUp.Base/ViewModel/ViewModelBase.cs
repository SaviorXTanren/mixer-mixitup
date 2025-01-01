using MixItUp.Base.Util;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModels
{
    public class UIViewModelCommand : ViewModelBase, ICommand
    {
        public event EventHandler CanExecuteChanged;

        private Func<bool> canExecute;

        private Action execute;
        private Func<Task> asyncExecute;

        private Action<object> parameterExecute;
        private Func<object, Task> asyncParameterExecute;

        private UIViewModelBase viewModel;

        public UIViewModelCommand(Action execute, UIViewModelBase viewModel)
        {
            this.execute = execute;
            this.viewModel = viewModel;
        }

        public UIViewModelCommand(Func<bool> canExecute, Action execute, UIViewModelBase viewModel)
            : this(execute, viewModel)
        {
            this.canExecute = canExecute;
        }

        public UIViewModelCommand(Func<Task> asyncExecute, UIViewModelBase viewModel)
        {
            this.asyncExecute = asyncExecute;
            this.viewModel = viewModel;
        }

        public UIViewModelCommand(Func<bool> canExecute, Func<Task> asyncExecute, UIViewModelBase viewModel)
            : this(asyncExecute, viewModel)
        {
            this.canExecute = canExecute;
        }
        public UIViewModelCommand(Action<object> parameterExecute, UIViewModelBase viewModel)
        {
            this.parameterExecute = parameterExecute;
            this.viewModel = viewModel;
        }

        public UIViewModelCommand(Func<bool> canExecute, Action<object> parameterExecute, UIViewModelBase viewModel)
            : this(parameterExecute, viewModel)
        {
            this.canExecute = canExecute;
        }

        public UIViewModelCommand(Func<object, Task> asyncParameterExecute, UIViewModelBase viewModel)
        {
            this.asyncParameterExecute = asyncParameterExecute;
            this.viewModel = viewModel;
        }

        public UIViewModelCommand(Func<bool> canExecute, Func<object, Task> asyncParameterExecute, UIViewModelBase viewModel)
            : this(asyncParameterExecute, viewModel)
        {
            this.canExecute = canExecute;
        }

        public bool IsRunning
        {
            get { return this.isRunning; }
            private set
            {
                this.isRunning = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool isRunning = false;

        public bool CanExecute(object parameter)
        {
            try
            {
                if (this.canExecute != null)
                {
                    return this.canExecute();
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
                this.viewModel.StartLoadingOperation();
                this.IsRunning = true;
                if (this.execute != null)
                {
                    this.execute();
                }
                else if (this.asyncExecute != null)
                {
                    await this.asyncExecute();
                }
                else if (this.parameterExecute != null)
                {
                    this.parameterExecute(parameter);
                }
                else if (this.asyncParameterExecute != null)
                {
                    await this.asyncParameterExecute(parameter);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            finally
            {
                this.IsRunning = false;
                this.viewModel.EndLoadingOperation();
            }
        }

        public void NotifyCanExecuteChanged() { this.CanExecuteChanged.Invoke(this, new EventArgs()); }
    }

    public class UIViewModelBase : ViewModelBase
    {
        public event EventHandler StartLoadingOperationOccurred = delegate { };
        public event EventHandler EndLoadingOperationOccurred = delegate { };

        public bool IsLoading
        {
            get { return this.isLoading; }
            private set
            {
                this.isLoading = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("IsNotLoading");
            }
        }
        private bool isLoading = false;

        public bool IsNotLoading { get { return !this.IsLoading; } }

        private int loadingOperations = 0;

        public async Task OnOpen() { await this.RunAsync(async () => await this.OnOpenInternal()); }

        public async Task OnVisible() { await this.RunAsync(async () => await this.OnVisibleInternal()); }

        public async Task OnClosed() { await this.RunAsync(async () => await this.OnClosedInternal()); }

        public virtual void StartLoadingOperation()
        {
            this.loadingOperations++;
            if (this.loadingOperations == 1)
            {
                this.IsLoading = true;
                this.StartLoadingOperationOccurred?.Invoke(this, new EventArgs());
            }
        }

        public virtual void EndLoadingOperation()
        {
            this.loadingOperations = Math.Max(this.loadingOperations - 1, 0);
            if (this.loadingOperations == 0)
            {
                this.IsLoading = false;
                this.EndLoadingOperationOccurred?.Invoke(this, new EventArgs());
            }
        }

        protected virtual Task OnOpenInternal() { return Task.CompletedTask; }

        protected virtual Task OnVisibleInternal() { return Task.CompletedTask; }

        protected virtual Task OnClosedInternal() { return Task.CompletedTask; }

        protected async Task RunAsync(Func<Task> function)
        {
            this.StartLoadingOperation();
            await function();
            this.EndLoadingOperation();
        }

        protected async Task<T> RunAsyncWithResult<T>(Func<Task<T>> function)
        {
            this.StartLoadingOperation();
            T result = await function();
            this.EndLoadingOperation();
            return result;
        }

        protected ICommand CreateCommand(Action execute) { return new UIViewModelCommand(execute, this); }

        protected ICommand CreateCommand(Func<bool> canExecute, Action execute) { return new UIViewModelCommand(canExecute, execute, this); }

        protected ICommand CreateCommand(Func<Task> execute) { return new UIViewModelCommand(execute, this); }

        protected ICommand CreateCommand(Func<bool> canExecute, Func<Task> execute) { return new UIViewModelCommand(canExecute, execute, this); }

        protected ICommand CreateCommand(Action<object> execute) { return new UIViewModelCommand(execute, this); }

        protected ICommand CreateCommand(Func<bool> canExecute, Action<object> execute) { return new UIViewModelCommand(canExecute, execute, this); }

        protected ICommand CreateCommand(Func<object, Task> execute) { return new UIViewModelCommand(execute, this); }

        protected ICommand CreateCommand(Func<bool> canExecute, Func<object, Task> execute) { return new UIViewModelCommand(canExecute, execute, this); }

        protected int GetPositiveIntFromString(string value)
        {
            if (!string.IsNullOrEmpty(value) && int.TryParse(value, out int intValue) && intValue > 0)
            {
                return intValue;
            }
            return 0;
        }

        protected double GetPositiveDoubleFromString(string value)
        {
            if (!string.IsNullOrEmpty(value) && double.TryParse(value, out double doubleValue) && doubleValue > 0)
            {
                return doubleValue;
            }
            return 0;
        }
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

    public class ViewModelBase : NotifyPropertyChangedBase { }
}
