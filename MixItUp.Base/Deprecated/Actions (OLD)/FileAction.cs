using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [Obsolete]
    public enum FileActionTypeEnum
    {
        SaveToFile,
        AppendToFile,
        ReadFromFile,
        ReadSpecificLineFromFile,
        ReadRandomLineFromFile,
        RemoveSpecificLineFromFile,
        RemoveRandomLineFromFile,
    }

    [Obsolete]
    [DataContract]
    public class FileAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return FileAction.asyncSemaphore; } }

        [DataMember]
        public FileActionTypeEnum FileActionType { get; set; }

        [DataMember]
        public string TransferText { get; set; }

        [DataMember]
        public string LineIndexToRead { get; set; }

        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        [Obsolete]
        public bool SaveToFile { get; set; }

        public FileAction() : base(ActionTypeEnum.File) { }

        public FileAction(FileActionTypeEnum fileActionType, string transferText, string filePath)
            : this()
        {
            this.FileActionType = fileActionType;
            this.TransferText = transferText;
            this.FilePath = filePath;
        }

        protected override Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            return Task.FromResult(0);
        }
    }
}
