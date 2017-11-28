using Mixer.Base.Model.Channel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface ISettingsService
    {
        Task<IEnumerable<IChannelSettings>> GetAllSettings();

        IChannelSettings Create(ExpandedChannelModel channel, bool isStreamer);

        void Initialize(IChannelSettings settings);

        Task Save(IChannelSettings settings);

        Task Save(IChannelSettings settings, string filePath);

        Task<bool> SaveAndValidate(IChannelSettings settings);

        Task SaveBackup(IChannelSettings settings);

        string GetFilePath(IChannelSettings settings);
    }
}
