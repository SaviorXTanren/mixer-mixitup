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

        public const string CSharpDefaultScriptTemplate =
@"using System;
            
namespace CustomNamespace
{
    public class CustomClass
    {
        public object Run()
        {
            // Your code goes here
            System.Console.WriteLine(""Hello World!"");

            // Return any data here that you'd like to use
            return 0;
        }
    }
}";

        public const string PythonDefaultScriptTemplate =
@"def run():
    # Your code goes here
    print 'Hello World!'

    # Return any data here that you'd like to use
    return 0";

        public const string JavascriptDefaultScriptTemplate =
@"function run()
{
    // Your code goes here
    var text = 'Hello World!';

    // Return any data here that you'd like to use
    return 0;
}";

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