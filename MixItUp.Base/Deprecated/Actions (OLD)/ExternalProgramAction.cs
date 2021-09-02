using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [Obsolete]
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

        protected override Task PerformInternal(UserV2ViewModel user, IEnumerable<string> arguments)
        {
            return Task.CompletedTask;
        }
    }
}
