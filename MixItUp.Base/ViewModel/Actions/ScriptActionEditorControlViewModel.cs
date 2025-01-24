using MixItUp.Base.Model.Actions;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
{
    public class ScriptActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.Script; } }

        public IEnumerable<ScriptActionType> ActionTypes { get { return EnumHelper.GetEnumList<ScriptActionType>(); } }

        public ScriptActionType SelectedActionType
        {
            get { return this.selectedActionType; }
            set
            {
                this.selectedActionType = value;

                if (this.SelectedActionType == ScriptActionType.CSharp)
                {
                    this.Script = Resources.ScriptActionCSharpDefaultScriptTemplate;
                }
                else if (this.SelectedActionType == ScriptActionType.Python)
                {
                    this.Script = Resources.ScriptActionPythonDefaultScriptTemplate;
                }
                else if (this.SelectedActionType == ScriptActionType.Javascript)
                {
                    this.Script = Resources.ScriptActionJavascriptDefaultScriptTemplate;
                }
            }
        }
        private ScriptActionType selectedActionType;

        public string Script
        {
            get { return this.script; }
            set
            {
                this.script = value;
                this.NotifyPropertyChanged();
            }
        }
        private string script;

        public ScriptActionEditorControlViewModel(ScriptActionModel action)
            : base(action)
        {
            this.selectedActionType = action.ActionType;
            this.Script = action.Script;
        }

        public ScriptActionEditorControlViewModel()
            : base()
        {
            this.SelectedActionType = ScriptActionType.CSharp;
        }

        public override Task<Result> Validate()
        {
            if (string.IsNullOrEmpty(this.Script))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.ScriptActionMissingScript));
            }
            return Task.FromResult(new Result());
        }

        protected override Task<ActionModelBase> GetActionInternal() { return Task.FromResult<ActionModelBase>(new ScriptActionModel(this.SelectedActionType, this.Script)); }
    }
}