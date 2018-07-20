using MixItUp.Base.ViewModel.User;
using System;
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
        public const string CounterFolderName = "Counters";

        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return CounterAction.asyncSemaphore; } }

        [DataMember]
        public string CounterName { get; set; }

        [DataMember]
        [Obsolete]
        public int CounterAmount { get; set; }
        [DataMember]
        public string Amount { get; set; }

        [DataMember]
        public bool UpdateAmount { get; set; }

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

        public CounterAction(string counterName, string amount, bool saveToFile, bool resetOnLoad)
            : this()
        {
            this.CounterName = counterName;
            this.UpdateAmount = true;
            this.Amount = amount;
            this.SaveToFile = saveToFile;
            this.ResetOnLoad = resetOnLoad;
        }

        public async Task SetCounterValue()
        {
            if (!ChannelSession.Counters.ContainsKey(this.CounterName))
            {
                ChannelSession.Counters[this.CounterName] = 0;

                if (File.Exists(this.GetCounterFilePath()))
                {
                    string data = await ChannelSession.Services.FileService.ReadFile(this.GetCounterFilePath());
                    if (double.TryParse(data, out double amount))
                    {
                        ChannelSession.Counters[this.CounterName] = amount;
                    }
                }

                if (this.ResetOnLoad)
                {
                    ChannelSession.Counters[this.CounterName] = 0.0;
                }

                await this.SaveCounterToFile();
            }
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (!ChannelSession.Counters.ContainsKey(this.CounterName))
            {
                ChannelSession.Counters[this.CounterName] = 0.0;
            }

            if (this.UpdateAmount)
            {
                string amountText = await this.ReplaceStringWithSpecialModifiers(this.Amount, user, arguments);
                if (double.TryParse(amountText, out double amount))
                {
                    ChannelSession.Counters[this.CounterName] += amount;
                }
            }
            else if (this.ResetAmount)
            {
                ChannelSession.Counters[this.CounterName] = 0.0;
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
