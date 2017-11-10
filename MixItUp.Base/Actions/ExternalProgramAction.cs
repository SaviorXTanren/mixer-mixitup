using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class ExternalProgramAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSempahore { get { return ExternalProgramAction.asyncSemaphore; } }

        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public string Arguments { get; set; }

        [DataMember]
        public bool ShowWindow { get; set; }

        public ExternalProgramAction() : base(ActionTypeEnum.ExternalProgram) { }

        public ExternalProgramAction(string filePath, string arguments, bool showWindow)
            : this()
        {
            this.FilePath = filePath;
            this.Arguments = arguments;
            this.ShowWindow = showWindow;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
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
