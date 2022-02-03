using MixItUp.Base.ViewModels;
using System;

namespace MixItUp.Base.ViewModel.Tutorials
{
    public class TutorialStepViewModel : UIViewModelBase
    {
        public string ID { get; set; }

        public string Header { get; set; }
        public string Content { get; set; }

        public Action OnEntering { get; set; }
        public Action OnEnter { get; set; }
        public Action OnExit { get; set; }

        public Action DoStep { get; set; }
        public Func<bool> CanDoStep { get; set; } = () => { return true; };

        public TutorialStepViewModel(string id, string header, string content)
        {
            this.ID = id;
            this.Header = header;
            this.Content = content;
        }

        public TutorialStepViewModel(string id, string header, string content, Action doStep)
            : this(id, header, content)
        {
            this.DoStep = doStep;
        }

        public TutorialStepViewModel(string id, string header, string content, Action doStep, Func<bool> canDoStep)
            : this(id, header, content, doStep)
        {
            this.CanDoStep = canDoStep;
        }

        public bool HasDo { get { return this.DoStep != null; } }

        public bool HasOnEntering { get { return this.OnEntering != null; } }
        public bool HasOnEnter { get { return this.OnEnter != null; } }
        public bool HasOnExist { get { return this.OnExit != null; } }
    }
}
