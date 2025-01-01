using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.WPF.Services
{
    public class WindowsScriptRunnerService : IScriptRunnerService
    {
        public async Task<string> RunCSharpCode(CommandParametersModel parameters, string code)
        {
            CompilerResults compileResults = await this.CompileDotNetCode(CodeDomProvider.CreateProvider("CSharp"), parameters, code);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            return await AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
            {
                try
                {
                    object o = compileResults.CompiledAssembly.CreateInstance("CustomNamespace.CustomClass");
                    MethodInfo mi = o.GetType().GetMethod("Run");
                    object result = mi.Invoke(o, null);

                    if (result != null)
                    {
                        return result.ToString();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.ScriptActionFailedCompile, ex.ToString()), parameters.Platform);
                }
                return null;
            }, cancellationTokenSource.Token);
        }

        public async Task<string> RunVisualBasicCode(CommandParametersModel parameters, string code)
        {
            await this.CompileDotNetCode(CodeDomProvider.CreateProvider("VisualBasic"), parameters, code);
            return null;
        }

        private async Task<CompilerResults> CompileDotNetCode(CodeDomProvider provider, CommandParametersModel parameters, string code)
        {
            try
            {
                CompilerParameters compilerParams = new CompilerParameters
                {
                    GenerateInMemory = true,
                    GenerateExecutable = false,
                    TreatWarningsAsErrors = false
                };

                CompilerResults compileResults = provider.CompileAssemblyFromSource(compilerParams, code);

                if (compileResults.Errors.Count > 0)
                {
                    await ServiceManager.Get<ChatService>().SendMessage(
                        string.Format(MixItUp.Base.Resources.ScriptActionFailedCompile, string.Join(", ", compileResults.Errors)),
                        parameters.Platform);

                    return null;
                }

                return compileResults;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return null;
        }
    }
}
