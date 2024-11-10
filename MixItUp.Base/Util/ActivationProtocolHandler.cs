using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel.Commands;
using MixItUp.Base.Util;
using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Util
{
    /// <summary>
    /// https://css-tricks.com/hyperlinking-beyond-the-web/
    /// https://www.meziantou.net/registering-an-application-to-a-uri-scheme-using-net.htm
    /// https://blog.magnusmontin.net/2019/05/10/handle-protocol-activation-and-redirection-in-packaged-apps/
    /// </summary>
    public static class ActivationProtocolHandler
    {
        public const string PipeName = "mixitup";

        public const string FileAssociationProgramID = "MixItUp.MIUCommand.1";

        public const string URIProtocolActivationHeader = "mixitup";
        public const string URIProtocolActivationCommunityCommand = URIProtocolActivationHeader + "://community/command/";

        public static event EventHandler<Guid> OnCommunityCommandActivation = delegate { };
        public static event EventHandler<CommandModelBase> OnCommandFileActivation = delegate { };

        private static Task activationHandlerTask;
        private static CancellationTokenSource activationHandlerTaskCancellationTokenSource = new CancellationTokenSource();

        public static void Initialize()
        {
            Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        using (NamedPipeServerStream pipeServer = new NamedPipeServerStream(PipeName, PipeDirection.In))
                        {
                            pipeServer.WaitForConnection();

                            try
                            {
                                byte[] responseBytes = new byte[1000000];
                                int count = await pipeServer.ReadAsync(responseBytes, 0, responseBytes.Length);
                                string response = Encoding.ASCII.GetString(responseBytes, 0, count);

                                if (response.StartsWith(URIProtocolActivationCommunityCommand))
                                {
                                    if (Guid.TryParse(response.Replace(URIProtocolActivationCommunityCommand, ""), out Guid commandID))
                                    {
                                        ActivationProtocolHandler.OnCommunityCommandActivation(null, commandID);
                                    }
                                }
                                else if (response.EndsWith(CommandEditorWindowViewModelBase.MixItUpCommandFileExtension))
                                {
                                    CommandModelBase command = await CommandEditorWindowViewModelBase.ImportCommandFromFile(response);
                                    if (command != null)
                                    {
                                        ActivationProtocolHandler.OnCommandFileActivation(null, command);
                                    }
                                }
                            }
                            catch (IOException ex)
                            {
                                Logger.Log(LogLevel.Error, "Named Pipe Server Error: " + ex);
                            }
                        }
                    }
                }
                catch (ThreadAbortException) { return; }
                catch (OperationCanceledException) { return; }
                catch (Exception ex) { Logger.Log(ex); }
            }, ActivationProtocolHandler.activationHandlerTaskCancellationTokenSource.Token);
        }

        public static void Close()
        {
            if (ActivationProtocolHandler.activationHandlerTask != null)
            {
                ActivationProtocolHandler.activationHandlerTask = null;

                ActivationProtocolHandler.activationHandlerTaskCancellationTokenSource.Cancel();
                ActivationProtocolHandler.activationHandlerTaskCancellationTokenSource = null;
            }
        }

        public static void SendRequest(string[] args)
        {
            if (args != null)
            {
                try
                {
                    using (NamedPipeClientStream namedPipeClient = new NamedPipeClientStream(".", PipeName, PipeDirection.Out))
                    {
                        byte[] requestBytes = Encoding.UTF8.GetBytes(string.Join(" ", args));

                        namedPipeClient.Connect();
                        namedPipeClient.Write(requestBytes, 0, requestBytes.Length);
                    }
                }
                catch (Exception) { }
            }
        }
    }
}
