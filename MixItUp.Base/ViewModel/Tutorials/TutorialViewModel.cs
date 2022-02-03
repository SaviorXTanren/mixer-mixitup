using System.Collections.Generic;

namespace MixItUp.Base.ViewModel.Tutorials
{
    public class TutorialViewModel
    {
        public string Name { get; set; }

        public List<TutorialStepViewModel> Steps { get; set; } = new List<TutorialStepViewModel>();

        public TutorialViewModel(string name)
        {
            this.Name = name;
        }
    }
}
