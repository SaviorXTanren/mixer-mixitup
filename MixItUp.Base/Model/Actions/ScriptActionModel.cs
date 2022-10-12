using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using StreamingClient.Base.Util;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum ScriptActionType
    {
        CSharp,
        Python,
        Javascript
    }

    [DataContract]
    public class ScriptActionModel : ActionModelBase
    {
        public const string OutputSpecialIdentifier = "scriptresult";

        [DataMember]
        public ScriptActionType ActionType { get; set; }

        [DataMember]
        public string Script { get; set; }

        public ScriptActionModel(ScriptActionType actionType, string script)
            : base(ActionTypeEnum.Script)
        {
            this.ActionType = actionType;
            this.Script = script;
        }

        [Obsolete]
        public ScriptActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            string script = await ReplaceStringWithSpecialModifiers(this.Script, parameters);
            try
            {
                string result = null;
                if (this.ActionType == ScriptActionType.CSharp)
                {
                    result = await ServiceManager.Get<IScriptRunnerService>().RunCSharpCode(parameters, script);
                }
                else if (this.ActionType == ScriptActionType.Python)
                {
                    result = await ServiceManager.Get<IScriptRunnerService>().RunPythonCode(parameters, script);
                }
                else if (this.ActionType == ScriptActionType.Javascript)
                {
                    result = await ServiceManager.Get<IScriptRunnerService>().RunJavascriptCode(parameters, script);
                }

                if (!string.IsNullOrEmpty(result))
                {
                    parameters.SpecialIdentifiers[OutputSpecialIdentifier] = result;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
    }
}