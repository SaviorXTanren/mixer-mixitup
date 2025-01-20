using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum ScriptActionType
    {
        CSharp,
        Python,
        Javascript
    }

    [DataContract]
    public class ScriptActionModel : ActionModelBase
    {
        public const string OutputSpecialIdentifier = "scriptresult";

        [DataMember]
        public ScriptActionType ActionType { get; set; }

        [DataMember]
        public string Script { get; set; }

        public ScriptActionModel(ScriptActionType actionType, string script)
            : base(ActionTypeEnum.Script)
        {
            this.ActionType = actionType;
            this.Script = script;
        }

        [Obsolete]
        public ScriptActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            string script = await ReplaceStringWithSpecialModifiers(this.Script, parameters);
            try
            {
                string result = null;
                if (this.ActionType == ScriptActionType.CSharp)
                {
                    result = await ServiceManager.Get<IScriptRunnerService>().RunCSharpCode(parameters, script);
                }
                else if (this.ActionType == ScriptActionType.Python)
                {
                    if (string.IsNullOrWhiteSpace(ChannelSession.Settings.PythonExecutablePath))
                    {
                        await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.ScriptActionPythonExecutablePathNotSet, parameters.Platform);
                        return;
                    }

                    string pythonFilePath = Path.Combine(ServiceManager.Get<IFileService>().GetTempFolder(), Guid.NewGuid().ToString() + ".py");
                    await ServiceManager.Get<IFileService>().SaveFile(pythonFilePath, script);

                    List<string> output = new List<string>();

                    Process process = new Process();
                    process.StartInfo = new ProcessStartInfo()
                    {
                        FileName = ChannelSession.Settings.PythonExecutablePath,
                        Arguments = $"-u \"{pythonFilePath}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                    };

                    process.EnableRaisingEvents = true;
                    process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                    {
                        if (e.Data != null)
                        {
                            output.Add(e.Data);
                        }
                    };
                    process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                    {
                        if (e.Data != null)
                        {
                            output.Add(e.Data);
                        }
                    };

                    process.Start();
                    process.BeginErrorReadLine();
                    process.BeginOutputReadLine();

                    await Task.Run(() =>
                    {
                        process.WaitForExit();
                    });

                    result = string.Join(Environment.NewLine, output);
                }
                else if (this.ActionType == ScriptActionType.Javascript)
                {
                    if (!ServiceManager.Get<OverlayV3Service>().IsConnected)
                    {
                        await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.ScriptActionOverlayNotEnableOrConnected, parameters.Platform);
                        return;
                    }

                    OverlayEndpointV3Service overlayEndpoint = ServiceManager.Get<OverlayV3Service>().GetDefaultOverlayEndpointService();
                    if (overlayEndpoint == null || overlayEndpoint.ConnectedClients == 0)
                    {
                        await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.ScriptActionOverlayNotEnableOrConnected, parameters.Platform);
                        return;
                    }

                    OverlayJavascriptScriptV3Model overlayItem = new OverlayJavascriptScriptV3Model(script);

                    overlayItem.ID = Guid.NewGuid();

                    string iframeHTML = overlayEndpoint.GetItemIFrameHTML();
                    iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, nameof(overlayItem.HTML), overlayItem.HTML);
                    iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, nameof(overlayItem.CSS), overlayItem.CSS);
                    iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, nameof(overlayItem.Javascript), overlayItem.Javascript);
                    iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, nameof(overlayItem.ID), overlayItem.ID.ToString());

                    overlayEndpoint.PacketListeningItems[overlayItem.ID] = overlayItem;

                    await overlayEndpoint.Add(overlayItem.ID.ToString(), iframeHTML);

                    result = await overlayItem.WaitForResult();

                    await overlayEndpoint.Remove(overlayItem.ID.ToString());
                }

                if (!string.IsNullOrEmpty(result))
                {
                    parameters.SpecialIdentifiers[OutputSpecialIdentifier] = result;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
    }
}