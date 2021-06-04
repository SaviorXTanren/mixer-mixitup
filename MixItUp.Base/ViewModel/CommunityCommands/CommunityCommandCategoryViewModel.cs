using MixItUp.Base.Model.Store;
using MixItUp.Base.ViewModels;
using System.Collections.Generic;

namespace MixItUp.Base.ViewModel.CommunityCommands
{
    public class CommunityCommandCategoryViewModel : UIViewModelBase
    {
        private CommunityCommandCategoryModel model;

        public CommunityCommandCategoryViewModel(CommunityCommandCategoryModel model)
        {
            this.model = model;

            foreach (CommunityCommandModel command in this.model.Commands)
            {
                this.Commands.Add(new CommunityCommandViewModel(command));
            }
        }

        public string Name { get { return this.model.Name; } }

        public string Description { get { return this.model.Description; } }

        public List<CommunityCommandViewModel> Commands { get; } = new List<CommunityCommandViewModel>();
    }
}
