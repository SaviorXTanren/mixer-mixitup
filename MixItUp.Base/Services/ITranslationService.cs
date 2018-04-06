using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface ITranslationService
    {
        Task SetAccessToken();

        Task<IEnumerable<CultureInfo>> GetAvailableLanguages();

        Task<string> Translate(CultureInfo language, string text, bool allowProfanity = true);
    }
}
