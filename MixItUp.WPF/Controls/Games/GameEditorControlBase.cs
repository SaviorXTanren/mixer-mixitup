using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Games
{
    public abstract class GameEditorControlBase : LoadingControlBase
    {
        public abstract Task<bool> Validate();

        public abstract void SaveGameCommand();
    }
}
