using MixItUp.Base.Model.Actions;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Actions
{
    public class MtionStudioInputParameterViewModel : UIViewModelBase
    {
        public string Name { get; set; }
        public MtionStudioParameterTypeEnum Type { get; set; }

        public string Display { get { return $"{this.Name} ({this.Type})"; } }

        public string Value
        {
            get { return this.value; }
            set
            {
                this.value = value;
                this.NotifyPropertyChanged();
            }
        }
        private string value;

        public MtionStudioInputParameterViewModel(MtionStudioTriggerParameter parameter)
        {
            this.Name = parameter.name;
            this.Type = parameter.Type;
        }

        public MtionStudioActionParameterModel ToModel()
        {
            return new MtionStudioActionParameterModel()
            {
                Type = this.Type,
                Value = this.Value,
            };
        }
    }

    public class MtionStudioActionViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.MtionStudio; } }

        public bool MtionStudioConnected { get { return ServiceManager.Get<MtionStudioService>().IsConnected; } }
        public bool MtionStudioNotConnected { get { return !this.MtionStudioConnected; } }

        public ObservableCollection<MtionStudioClubhouse> Clubhouses { get; set; } = new ObservableCollection<MtionStudioClubhouse>();

        public MtionStudioClubhouse SelectedClubhouse
        {
            get { return this.selectedClubhouse; }
            set
            {
                bool updateOccurred = value != this.selectedClubhouse;

                this.selectedClubhouse = value;
                this.NotifyPropertyChanged();

                if (updateOccurred && this.selectedClubhouse != null)
                {
                    this.Triggers.Clear();
                    this.Triggers.AddRange(this.SelectedClubhouse.external_trigger_datas);

                    this.SelectedTrigger = this.Triggers.FirstOrDefault();
                }
            }
        }
        private MtionStudioClubhouse selectedClubhouse;

        public ObservableCollection<MtionStudioTrigger> Triggers { get; set; } = new ObservableCollection<MtionStudioTrigger>();

        public MtionStudioTrigger SelectedTrigger
        {
            get { return this.selectedTrigger; }
            set
            {
                bool updateOccurred = value != this.selectedTrigger;

                this.selectedTrigger = value;
                this.NotifyPropertyChanged();

                if (updateOccurred)
                {
                    this.Parameters.Clear();
                    if (this.SelectedTrigger != null)
                    {
                        foreach (MtionStudioTriggerParameter parameter in value.output_parameters)
                        {
                            this.Parameters.Add(new MtionStudioInputParameterViewModel(parameter));
                        }
                    }
                    this.NotifyPropertyChanged(nameof(this.Parameters));
                }
            }
        }
        private MtionStudioTrigger selectedTrigger;

        public ObservableCollection<MtionStudioInputParameterViewModel> Parameters { get; set; } = new ObservableCollection<MtionStudioInputParameterViewModel>();

        public ICommand RefreshCacheCommand { get; set; }

        private string clubhouseID;
        private string triggerID;
        private IEnumerable<MtionStudioActionParameterModel> parameters;

        public MtionStudioActionViewModel(MtionStudioActionModel action)
            : base(action)
        {
            this.clubhouseID = action.ClubhouseID;
            this.triggerID = action.TriggerID;
            this.parameters = action.Parameters;
        }

        public MtionStudioActionViewModel() : base() { }

        public override Task<Result> Validate()
        {
            if (this.SelectedTrigger == null)
            {
                if (this.MtionStudioConnected || string.IsNullOrEmpty(this.triggerID))
                {
                    return Task.FromResult<Result>(new Result(MixItUp.Base.Resources.MtionStudioActionMissingTrigger));
                }
            }

            return Task.FromResult<Result>(new Result());
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            return Task.FromResult<ActionModelBase>(new MtionStudioActionModel(this.SelectedClubhouse?.id ?? this.clubhouseID, this.SelectedTrigger?.id ?? this.triggerID, this.Parameters.Select(p => p.ToModel())));
        }

        protected override async Task OnOpenInternal()
        {
            this.RefreshCacheCommand = this.CreateCommand(async () =>
            {
                await this.TryConnectToMtionStudio();

                if (this.MtionStudioConnected)
                {
                    ServiceManager.Get<MtionStudioService>().ClearCaches();
                }

                await this.LoadData();
            });

            await this.TryConnectToMtionStudio();

            await this.LoadData();

            if (!string.IsNullOrEmpty(this.clubhouseID) && !string.IsNullOrEmpty(this.triggerID))
            {
                this.SelectedClubhouse = this.Clubhouses.FirstOrDefault(c => string.Equals(c.id, this.clubhouseID));
                if (this.SelectedClubhouse != null)
                {
                    this.SelectedTrigger = this.Triggers.FirstOrDefault(t => string.Equals(t.id, this.triggerID));
                    if (this.SelectedTrigger != null && this.parameters != null)
                    {
                        for (int i = 0; i < this.Parameters.Count && i < this.parameters.Count(); i++)
                        {
                            this.Parameters[i].Value = this.parameters.ElementAt(i).Value;
                        }
                    }
                }
            }

            await base.OnOpenInternal();
        }

        private async Task<bool> TryConnectToMtionStudio()
        {
            if (ChannelSession.Settings.MtionStudioEnabled && !this.MtionStudioConnected)
            {
                Result result = await ServiceManager.Get<MtionStudioService>().Connect();
                return result.Success;
            }
            return false;
        }

        private async Task LoadData()
        {
            this.Clubhouses.Clear();
            this.Triggers.Clear();

            if (this.MtionStudioConnected)
            {
                IEnumerable<MtionStudioClubhouse> clubhouses = await ServiceManager.Get<MtionStudioService>().GetAllClubhouses();
                if (clubhouses != null)
                {
                    this.Clubhouses.AddRange(clubhouses);
                }
            }
        }
    }
}