using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
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

    public interface IOvrStreamService : IExternalService
    {
        Task UpdateVariables(string titleName, Dictionary<string, string> variables);
        Task HideTitle(string titleName);
        Task EnableTitle(string titleName);
        Task DisableTitle(string titleName);
        Task PlayTitle(string titleName, Dictionary<string, string> variables);
        Task<string> DownloadImage(string uri);
        Task<OvrStreamTitle[]> GetTitles();
    }
}
