using MixItUp.Base.Model.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public bool WaitForFinish { get; set; }
        [DataMember]
        public bool SaveOutput { get; set; }

        public ExternalProgramActionModel(string filePath, string arguments, bool showWindow, bool waitForFinish, bool saveOutput)
            : base(ActionTypeEnum.ExternalProgram)
        {
            this.FilePath = filePath;
            this.Arguments = arguments;
            this.ShowWindow = showWindow;
            this.WaitForFinish = waitForFinish;
            this.SaveOutput = saveOutput;
        }

#pragma warning disable CS0612 // Type or member is obsolete
        internal ExternalProgramActionModel(MixItUp.Base.Actions.ExternalProgramAction action)
            : base(ActionTypeEnum.ExternalProgram)
        {
            this.FilePath = action.FilePath;
            this.Arguments = action.Arguments;
            this.ShowWindow = action.ShowWindow;
            this.WaitForFinish = action.WaitForFinish;
        }
#pragma warning restore CS0612 // Type or member is obsolete

        private ExternalProgramActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            List<string> output = new List<string>();

            Process process = new Process();
            process.StartInfo.FileName = this.FilePath;
            process.StartInfo.Arguments = await ReplaceStringWithSpecialModifiers(this.Arguments, parameters);
            process.StartInfo.CreateNoWindow = !this.ShowWindow;
            process.StartInfo.WindowStyle = (!this.ShowWindow) ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal;
            process.StartInfo.UseShellExecute = false;
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
