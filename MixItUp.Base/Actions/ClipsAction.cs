using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Twitch.Base.Models.NewAPI.Clips;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class ClipsAction : ActionBase
    {
        public const string ClipURLSpecialIdentifier = "clipurl";

        public const string VideoFileContentLocatorType = "HlsStreaming";
        private const string FFMPEGExecutablePath = "ffmpeg-4.0-win32-static\\bin\\ffmpeg.exe";

        public static string GetFFMPEGExecutablePath() { return Path.Combine(ChannelSession.Services.FileService.GetApplicationDirectory(), ClipsAction.FFMPEGExecutablePath); }

        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return ClipsAction.asyncSemaphore; } }

        [DataMember]
        public bool IncludeDelay { get; set; }

        [DataMember]
        public bool ShowClipInfoInChat { get; set; }

        public ClipsAction() : base(ActionTypeEnum.Clips) { }

        public ClipsAction(bool includeDelay, bool showClipInfoInChat)
            : this()
        {
            this.IncludeDelay = includeDelay;
            this.ShowClipInfoInChat = showClipInfoInChat;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            ClipCreationModel clipCreation = await ChannelSession.TwitchUserConnection.CreateClip(ChannelSession.TwitchChannelNewAPI, this.IncludeDelay);
            if (clipCreation != null)
            {
                for (int i = 0; i < 12; i++)
                {
                    await Task.Delay(5000);

                    ClipModel clip = await ChannelSession.TwitchUserConnection.GetClip(clipCreation);
                    if (clip != null && !string.IsNullOrEmpty(clip.url))
                    {
                        await this.ProcessClip(clip);
                        return;
                    }
                }
            }
            await ChannelSession.Services.Chat.SendMessage(MixItUp.Base.Resources.ClipCreationFailed);
        }

        private async Task ProcessClip(ClipModel clip)
        {
            if (this.ShowClipInfoInChat)
            {
                await ChannelSession.Services.Chat.SendMessage("Clip Created: " + clip.url);
            }

            this.extraSpecialIdentifiers[ClipURLSpecialIdentifier] = clip.url;

//            if (this.DownloadClip)
//            {
//                if (!Directory.Exists(this.DownloadDirectory))
//                {
//                    string error = "ERROR: The download folder specified for Mixer Clips does not exist";
//                    Logger.Log(error);
//                    await ChannelSession.Services.Chat.Whisper(ChannelSession.GetCurrentUser(), error);
//                    return;
//                }

//                if (!ChannelSession.Services.FileService.FileExists(MixerClipsAction.GetFFMPEGExecutablePath()))
//                {
//                    string error = "ERROR: FFMPEG could not be found and the Mixer Clip can not be converted without it";
//                    Logger.Log(error);
//                    await ChannelSession.Services.Chat.Whisper(ChannelSession.GetCurrentUser(), error);
//                    return;
//                }

//                ClipLocatorModel clipLocator = clip.contentLocators.FirstOrDefault(cl => cl.locatorType.Equals(VideoFileContentLocatorType));
//                if (clipLocator != null)
//                {
//                    clipName = clipName.ToFilePathString();
//                    string destinationFile = Path.Combine(this.DownloadDirectory, clipName + ".mp4");
//                    if (File.Exists(destinationFile))
//                    {
//                        clipName += "-" + DateTimeOffset.Now.ToFriendlyDateTimeString().ToFilePathString();
//                        destinationFile = Path.Combine(this.DownloadDirectory, clipName + ".mp4");
//                    }

//#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
//                    Task.Run(async () =>
//                    {
//                        Process process = new Process();
//                        process.StartInfo.FileName = MixerClipsAction.GetFFMPEGExecutablePath();
//                        process.StartInfo.Arguments = string.Format("-i {0} -c copy -bsf:a aac_adtstoasc \"{1}\"", clipLocator.uri, destinationFile.ToFilePathString());
//                        process.StartInfo.RedirectStandardOutput = true;
//                        process.StartInfo.UseShellExecute = false;
//                        process.StartInfo.CreateNoWindow = true;

//                        process.Start();
//                        while (!process.HasExited)
//                        {
//                            await Task.Delay(500);
//                        }
//                    });
//#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
//                }
//            }
        }
    }
}
