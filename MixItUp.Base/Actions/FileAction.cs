using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class FileAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return FileAction.asyncSemaphore; } }

        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public bool SaveToFile { get; set; }

        [DataMember]
        public string TransferText { get; set; }

        public FileAction() : base(ActionTypeEnum.File) { }

        public FileAction(bool saveToFile, string transferText, string filePath)
            : this()
        {
            this.SaveToFile = saveToFile;
            this.TransferText = transferText;
            this.FilePath = filePath;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (this.SaveToFile)
            {
                SpecialIdentifierStringBuilder stringBuilder = new SpecialIdentifierStringBuilder(this.TransferText);
                await stringBuilder.ReplaceCommonSpecialModifiers(user, arguments);
                await ChannelSession.Services.FileService.SaveFile(this.FilePath, stringBuilder.ToString());
            }
            else
            {
                string data = await ChannelSession.Services.FileService.ReadFile(this.FilePath);
                if (!string.IsNullOrEmpty(data))
                {
                    SpecialIdentifierStringBuilder.AddCustomSpecialIdentifier(this.TransferText, data);
                }
            }
        }
    }
}
