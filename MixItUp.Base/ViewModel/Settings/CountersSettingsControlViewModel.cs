using MixItUp.Base.Model.Settings;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Settings
{
    public class CounterViewModel : UIViewModelBase
    {
        private CounterModel model;

        public string Name { get { return this.model.Name; } }

        public double Amount
        {
            get { return this.model.Amount; }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                this.model.SetAmount(value);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                this.NotifyPropertyChanged();
            }
        }

        public bool SaveToFile
        {
            get { return this.model.SaveToFile; }
            set
            {
                this.model.SaveToFile = value;
                this.NotifyPropertyChanged();
            }
        }

        public bool ResetOnLoad
        {
            get { return this.model.ResetOnLoad; }
            set
            {
                this.model.ResetOnLoad = value;
                this.NotifyPropertyChanged();
            }
        }

        public ICommand DeleteCommand { get; set; }

        private CountersSettingsControlViewModel viewModel;

        public CounterViewModel(CountersSettingsControlViewModel viewModel)
        {
            this.viewModel = viewModel;

            this.DeleteCommand = this.CreateCommand(() =>
            {
                this.viewModel.DeleteCounter(this);
            });
        }

        public CounterViewModel(CountersSettingsControlViewModel viewModel, CounterModel model)
            : this(viewModel)
        {
            this.model = model;
        }
    }

    public class CountersSettingsControlViewModel : UIViewModelBase
    {
        public ObservableCollection<CounterViewModel> Counters { get; set; } = new ObservableCollection<CounterViewModel>();

        public string NewCounterName
        {
            get { return this.newCounterName; }
            set
            {
                this.newCounterName = value;
                this.NotifyPropertyChanged();
            }
        }
        private string newCounterName;

        public ICommand AddNewCounterCommand { get; set; }

        public CountersSettingsControlViewModel()
        {
            this.AddNewCounterCommand = this.CreateCommand(async () =>
            {
                if (string.IsNullOrEmpty(this.NewCounterName))
                {
                    await DialogHelper.ShowMessage(MixItUp.Base.Resources.NameRequired);
                    return;
                }

                this.NewCounterName = this.NewCounterName.Replace("$", "");
                if (!SpecialIdentifierStringBuilder.IsValidSpecialIdentifier(this.NewCounterName))
                {
                    await DialogHelper.ShowMessage(MixItUp.Base.Resources.NameInvalidAlphaNumericOnly);
                    return;
                }

                CounterModel.CreateCounter(this.NewCounterName, false, false);

                this.NewCounterName = string.Empty;

                this.RefreshList();
            });
        }

        public void DeleteCounter(CounterViewModel counter)
        {
            ChannelSession.Settings.Counters.Remove(counter.Name);
            this.RefreshList();
        }

        protected override Task OnOpenInternal()
        {
            this.RefreshList();
            return base.OnOpenInternal();
        }

        protected override Task OnVisibleInternal()
        {
            this.RefreshList();
            return base.OnVisibleInternal();
        }

        private void RefreshList()
        {
            this.Counters.Clear();
            foreach (var kvp in ChannelSession.Settings.Counters.ToList().OrderBy(c => c.Value.Name))
            {
                this.Counters.Add(new CounterViewModel(this, kvp.Value));
            }
        }
    }
}
