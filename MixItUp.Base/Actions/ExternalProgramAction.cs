using Mixer.Base.ViewModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public class ExternalProgramAction : ActionBase
    {
        public string FilePath { get; set; }

        public string Arguments { get; set; }

        public bool ShowWindow { get; set; }

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
                await this.Wait500();
            }
        }

        public override SerializableAction Serialize()
        {
            return new SerializableAction()
            {
                Type = this.Type,
                Values = new List<string>() { this.FilePath, this.Arguments, this.ShowWindow.ToString() }
            };
        }
    }
}
