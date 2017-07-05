using System.Diagnostics;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public class ExternalProgramAction : ActionBase
    {
        public string FilePath { get; set; }

        public string Arguments { get; set; }

        public bool ShowWindow { get; set; }

        public ExternalProgramAction() : base("External Program") { }

        public override async Task Perform()
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
    }
}
