using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class CounterAction : ActionBase
    {
        private const string CounterFolderName = "Counters";

        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return CounterAction.asyncSemaphore; } }

        [DataMember]
        public string CounterName { get; set; }

        [DataMember]
        public bool UpdateAmount { get; set; }
        [DataMember]
        public int CounterAmount { get; set; }

        [DataMember]
        public bool ResetAmount { get; set; }

        [DataMember]
        public bool SaveToFile { get; set; }
        [DataMember]
        public bool ResetOnLoad { get; set; }

        public CounterAction() : base(ActionTypeEnum.Counter) { }

        public CounterAction(string counterName, bool saveToFile, bool resetOnLoad)
            : this()
        {
            this.CounterName = counterName;
            this.ResetAmount = true;
            this.SaveToFile = saveToFile;
            this.ResetOnLoad = resetOnLoad;
        }

        public CounterAction(string counterName, int counterAmount, bool saveToFile, bool resetOnLoad)
            : this()
        {
            this.CounterName = counterName;
            this.UpdateAmount = true;
            this.CounterAmount = counterAmount;
            this.SaveToFile = saveToFile;
            this.ResetOnLoad = resetOnLoad;
        }

        public async Task SetCounterValue()
        {
            if (!ChannelSession.Counters.ContainsKey(this.CounterName))
            {
                ChannelSession.Counters[this.CounterName] = 0;
                if (this.ResetOnLoad)
                {
                    if (File.Exists(this.GetCounterFilePath()))
                    {
                        string data = await ChannelSession.Services.FileService.ReadFile(this.GetCounterFilePath());
                        if (int.TryParse(data, out int amount))
                        {
                            ChannelSession.Counters[this.CounterName] = amount;
                        }
                    }
                }
                await this.SaveCounterToFile();
            }
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (!ChannelSession.Counters.ContainsKey(this.CounterName))
            {
                ChannelSession.Counters[this.CounterName] = 0;
            }

            if (this.UpdateAmount)
            {
                ChannelSession.Counters[this.CounterName] += this.CounterAmount;
            }
            else if (this.ResetAmount)
            {
                ChannelSession.Counters[this.CounterName] = 0;
            }

            await this.SaveCounterToFile();
        }

        private async Task SaveCounterToFile()
        {
            if (this.SaveToFile)
            {
                await ChannelSession.Services.FileService.CreateDirectory(CounterAction.CounterFolderName);
                await ChannelSession.Services.FileService.SaveFile(this.GetCounterFilePath(), ChannelSession.Counters[this.CounterName].ToString());
            }
        }

        private string GetCounterFilePath() { return Path.Combine(CounterAction.CounterFolderName, this.CounterName + ".txt"); }
    }
}
