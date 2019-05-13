using Mixer.Base.Model.OAuth;
using MixItUp.Base.Model.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IOvrStreamService
    {
        Task<bool> Connect();

        Task Disconnect();

        Task UpdateVariables(string titleName, Dictionary<string, string> variables);
        Task HideTitle(string titleName);
        Task PlayTitle(string titleName, Dictionary<string, string> variables);
    }
}
