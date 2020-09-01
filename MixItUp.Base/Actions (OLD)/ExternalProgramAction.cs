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

        protected override SemaphoreSlim AsyncSemaphore { get { return ExternalProgramAction.asyncSemaphore; } }

        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public string Arguments { get; set; }

        [DataMember]
        public bool ShowWindow { get; set; }

        [DataMember]
        public bool WaitForFinish { get; set; }

        public ExternalProgramAction() : base(ActionTypeEnum.ExternalProgram) { }

        public ExternalProgramAction(string filePath, string arguments, bool showWindow, bool waitForFinish)
            : this()
        {
            this.FilePath = filePath;
            this.Arguments = arguments;
            this.ShowWindow = showWindow;
            this.WaitForFinish = waitForFinish;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            Process process = new Process();
            process.StartInfo.FileName = this.FilePath;
            process.StartInfo.Arguments = await this.ReplaceStringWithSpecialModifiers(this.Arguments, user, arguments);
            process.StartInfo.CreateNoWindow = !this.ShowWindow;
            process.StartInfo.WindowStyle = (!this.ShowWindow) ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal;
            process.StartInfo.UseShellExecute = false;

            process.Start();
            if (this.WaitForFinish)
            {
                while (!process.HasExited)
                {
                    await Task.Delay(500);
                }
            }
        }
    }
}
