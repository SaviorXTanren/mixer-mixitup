using Mixer.Base.Model.Channel;
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
        Task<IEnumerable<IChannelSettings>> GetAllSettings();

        Task<IChannelSettings> Create(ExpandedChannelModel channel, bool isStreamer);

        Task Initialize(IChannelSettings settings);

        Task Save(IChannelSettings settings);

        Task Save(IChannelSettings settings, string filePath);

        Task<bool> SaveAndValidate(IChannelSettings settings);

        Task SaveBackup(IChannelSettings settings, string filePath);

        Task PerformBackupIfApplicable(IChannelSettings settings);

        string GetFilePath(IChannelSettings settings);

        Task ClearAllUserData(IChannelSettings settings);
    }
}
