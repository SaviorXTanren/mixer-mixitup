using MixItUp.Base.ViewModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class ExternalProgramAction : ActionBase
    {
        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public string Arguments { get; set; }

        [DataMember]
        public bool ShowWindow { get; set; }

        public ExternalProgramAction() { }

        public ExternalProgramAction(string filePath, string arguments, bool showWindow)
            : base(ActionTypeEnum.ExternalProgram)
        {
            this.FilePath = filePath;
            this.Arguments = arguments;
            this.ShowWindow = showWindow;
        }

        public override async Task Perform(UserViewModel user, IEnumerable<string> arguments)
        {
            Process process = new Process();
            process.StartInfo.FileName = this.FilePath;
            process.StartInfo.Arguments = this.Arguments;
            process.StartInfo.CreateNoWindow = !this.ShowWindow;

            process.Start();
            while (!process.HasExited)
            {
                await Task.Delay(500);
            }
        }
    }
}
