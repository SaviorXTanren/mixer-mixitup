using MixItUp.Base.Model.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    [DataContract]
    public class ExternalProgramActionModel : ActionModelBase
    {
        public const string OutputSpecialIdentifier = "externalprogramresult";

        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public string Arguments { get; set; }

        [DataMember]
        public bool ShowWindow { get; set; }
        [DataMember]
        public bool ShellExecute { get; set; }
        [DataMember]
        public bool WaitForFinish { get; set; }
        [DataMember]
        public bool SaveOutput { get; set; }

        public ExternalProgramActionModel(string filePath, string arguments, bool showWindow, bool shellExecute, bool waitForFinish, bool saveOutput)
            : base(ActionTypeEnum.ExternalProgram)
        {
            this.FilePath = filePath;
            this.Arguments = arguments;
            this.ShowWindow = showWindow;
            this.ShellExecute = shellExecute;
            this.WaitForFinish = waitForFinish;
            this.SaveOutput = saveOutput;
        }

        [Obsolete]
        public ExternalProgramActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            List<string> output = new List<string>();

            Process process = new Process();
            process.StartInfo.FileName = await ReplaceStringWithSpecialModifiers(this.FilePath, parameters);
            process.StartInfo.Arguments = await ReplaceStringWithSpecialModifiers(this.Arguments, parameters);
            process.StartInfo.WorkingDirectory = Path.GetDirectoryName(process.StartInfo.FileName);
            process.StartInfo.CreateNoWindow = !this.ShowWindow;
            process.StartInfo.WindowStyle = (!this.ShowWindow) ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal;
            process.StartInfo.UseShellExecute = this.ShellExecute;
            if (this.WaitForFinish && this.SaveOutput)
            {
                process.StartInfo.RedirectStandardOutput = true;
                process.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        output.Add(e.Data);
                    }
                };
                process.StartInfo.RedirectStandardError = true;
                process.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        output.Add(e.Data);
                    }
                };
            }

            process.Start();
            if (this.WaitForFinish)
            {
                if (this.SaveOutput)
                {
                    process.BeginOutputReadLine();
                }

                while (!process.HasExited)
                {
                    await Task.Delay(500);
                }

                if (this.SaveOutput)
                {
                    parameters.SpecialIdentifiers[ExternalProgramActionModel.OutputSpecialIdentifier] = string.Join(Environment.NewLine, output);
                }
            }
        }
    }
}
