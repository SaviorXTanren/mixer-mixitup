using MixItUp.Base.Model.Commands;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IScriptRunnerService
    {
        Task<string> RunCSharpCode(CommandParametersModel parameters, string code);

        Task<string> RunVisualBasicCode(CommandParametersModel parameters, string code);
    }
}
