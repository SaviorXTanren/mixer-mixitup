using Mixer.Base.Model.Channel;
using MixItUp.Base.Model.Settings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public enum SettingsBackupRateEnum
    {
        None = 0,
        Daily,
        Weekly,
        Monthly,
    }

    public interface ISettingsService
    {
        Task<IEnumerable<SettingsV2Model>> GetAllSettings();

        Task<SettingsV2Model> Create(ExpandedChannelModel channel, bool isStreamer);

        Task Initialize(SettingsV2Model settings);

        Task Save(SettingsV2Model settings);

        Task<bool> SaveAndValidate(SettingsV2Model settings);

        Task SavePackagedBackup(SettingsV2Model settings, string filePath);

        Task PerformBackupIfApplicable(SettingsV2Model settings);

        string GetFilePath(SettingsV2Model settings);

        Task ClearAllUserData(SettingsV2Model settings);

        Task<int> GetSettingsVersion(string filePath);
        int GetLatestVersion();
    }
}
