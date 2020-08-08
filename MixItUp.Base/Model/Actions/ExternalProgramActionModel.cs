using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    [DataContract]
    public class ExternalProgramActionModel : ActionModelBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return ExternalProgramActionModel.asyncSemaphore; } }

        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public string Arguments { get; set; }

        [DataMember]
        public bool ShowWindow { get; set; }
        [DataMember]
        public bool WaitForFinish { get; set; }

        public ExternalProgramActionModel(string filePath, string arguments, bool showWindow, bool waitForFinish)
            : base(ActionTypeEnum.ExternalProgram)
        {
            this.FilePath = filePath;
            this.Arguments = arguments;
            this.ShowWindow = showWindow;
            this.WaitForFinish = waitForFinish;
        }

        internal ExternalProgramActionModel(MixItUp.Base.Actions.ExternalProgramAction action)
            : base(ActionTypeEnum.ExternalProgram)
        {
            this.FilePath = action.FilePath;
            this.Arguments = action.Arguments;
            this.ShowWindow = action.ShowWindow;
            this.WaitForFinish = action.WaitForFinish;
        }

        protected override async Task PerformInternal(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            Process process = new Process();
            process.StartInfo.FileName = this.FilePath;
            process.StartInfo.Arguments = await this.ReplaceStringWithSpecialModifiers(this.Arguments, user, platform, arguments, specialIdentifiers);
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
