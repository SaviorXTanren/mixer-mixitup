using Mixer.Base.Model.OAuth;
using MixItUp.Base.Model.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public class OvrStreamTitle
    {
        public string Name { get; set; }

        public OvrStreamVariable[] Variables { get; set; }
    }

    public class OvrStreamVariable
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public interface IOvrStreamService
    {
        Task<bool> Connect();

        Task Disconnect();

        Task UpdateVariables(string titleName, Dictionary<string, string> variables);
        Task HideTitle(string titleName);
        Task PlayTitle(string titleName, Dictionary<string, string> variables);
        Task<string> DownloadImage(string uri);
        Task<OvrStreamTitle[]> GetTitles();
    }
}
