using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Requirements
{
    public class ArgumentsRequirementItemViewModel : UIViewModelBase
    {
        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                this.NotifyPropertyChanged();
            }
        }
        private string name;

        public IEnumerable<ArgumentsRequirementItemTypeEnum> ArgumentTypes { get { return EnumHelper.GetEnumList<ArgumentsRequirementItemTypeEnum>(); } }

        public ArgumentsRequirementItemTypeEnum SelectedArgumentType
        {
            get { return selectedArgumentType; }
            set
            {
                this.selectedArgumentType = value;
                this.NotifyPropertyChanged();
            }
        }
        private ArgumentsRequirementItemTypeEnum selectedArgumentType = ArgumentsRequirementItemTypeEnum.User;

        public bool Optional
        {
            get { return this.optional; }
            set
            {
                this.optional = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool optional;

        public ICommand DeleteCommand { get; private set; }

        private ArgumentsRequirementViewModel viewModel;

        public ArgumentsRequirementItemViewModel(ArgumentsRequirementViewModel viewModel, ArgumentsRequirementItemModel requirement)
    : this(viewModel)
        {
            this.Name = requirement.Name;
            this.SelectedArgumentType = requirement.Type;
            this.Optional = requirement.Optional;
        }

        public ArgumentsRequirementItemViewModel(ArgumentsRequirementViewModel viewModel)
        {
            this.viewModel = viewModel;

            this.DeleteCommand = this.CreateCommand(() =>
            {
                this.viewModel.Delete(this);
            });
        }

        public Task<Result> Validate()
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.ArgumentsRequirementValidName));
            }
            return Task.FromResult(new Result());
        }

        public ArgumentsRequirementItemModel GetArgumentItem() { return new ArgumentsRequirementItemModel(this.Name, this.SelectedArgumentType, this.Optional); }
    }

    public class ArgumentsRequirementViewModel : RequirementViewModelBase
    {
        public ObservableCollection<ArgumentsRequirementItemViewModel> Items { get; set; } = new ObservableCollection<ArgumentsRequirementItemViewModel>();

        public ICommand AddItemCommand { get; private set; }

        public bool AssignToSpecialIdentifiers
        {
            get { return this.assignToSpecialIdentifiers; }
            set
            {
                this.assignToSpecialIdentifiers = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool assignToSpecialIdentifiers;

        public ArgumentsRequirementViewModel(ArgumentsRequirementModel arguments)
            : this()
        {
            if (arguments != null)
            {
                foreach (ArgumentsRequirementItemModel argument in arguments.Items)
                {
                    this.Items.Add(new ArgumentsRequirementItemViewModel(this, argument));
                }
                this.AssignToSpecialIdentifiers = arguments.AssignToSpecialIdentifiers;
            }
        }

        public ArgumentsRequirementViewModel()
        {
            this.AddItemCommand = this.CreateCommand(() =>
            {
                this.Items.Add(new ArgumentsRequirementItemViewModel(this));
            });
        }

        public void Delete(ArgumentsRequirementItemViewModel argument)
        {
            this.Items.Remove(argument);
        }

        public override async Task<Result> Validate()
        {
            foreach (ArgumentsRequirementItemViewModel item in this.Items)
            {
                Result result = await item.Validate();
                if (!result.Success)
                {
                    return result;
                }
            }
            return new Result();
        }

        public override RequirementModelBase GetRequirement()
        {
            return new ArgumentsRequirementModel(this.Items.Select(i => i.GetArgumentItem()), this.AssignToSpecialIdentifiers);
        }
    }
}
