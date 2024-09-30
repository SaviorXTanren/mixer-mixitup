using MixItUp.Base.Services;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Settings
{
    [DataContract]
    public class CounterModel
    {
        public const string CounterFolderName = "Counters";

        public static event EventHandler<CounterModel> OnCounterUpdated = delegate { };
        public static void CounterUpdated(CounterModel counter) { OnCounterUpdated(null, counter); }

        public static void CreateCounter(string name, bool saveToFile, bool resetOnLoad)
        {
            string counterName = name.ToLower();
            if (!ChannelSession.Settings.Counters.ContainsKey(counterName))
            {
                ChannelSession.Settings.Counters[counterName] = new CounterModel(counterName);
            }
            CounterModel counter = ChannelSession.Settings.Counters[counterName];
            counter.SaveToFile = saveToFile;
            counter.ResetOnLoad = resetOnLoad;
        }

        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public double Amount { get; set; }
        [DataMember]
        public bool SaveToFile { get; set; }
        [DataMember]
        public bool ResetOnLoad { get; set; }

        public CounterModel() { }

        public CounterModel(string name)
        {
            this.Name = name;
        }

        public string GetCounterFilePath() { return Path.Combine(CounterModel.CounterFolderName, this.Name + ".txt"); }

        public async Task SetAmount(double amount)
        {
            this.Amount = amount;
            if (this.SaveToFile)
            {
                await this.SaveAmountToFile();
            }
            CounterUpdated(this);
        }

        public async Task UpdateAmount(double amount) { await this.SetAmount(this.Amount + amount); }

        public async Task ResetAmount() { await this.SetAmount(0); }

        private async Task SaveAmountToFile()
        {
            await ServiceManager.Get<IFileService>().CreateDirectory(CounterModel.CounterFolderName);
            await ServiceManager.Get<IFileService>().SaveFile(this.GetCounterFilePath(), this.Amount.ToString());
        }
    }
}
