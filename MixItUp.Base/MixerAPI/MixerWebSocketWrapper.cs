using System.Threading.Tasks;

namespace MixItUp.Base.MixerAPI
{
    public abstract class MixerWebSocketWrapper : MixerRequestWrapperBase
    {
        public async Task<bool> AttemptConnect(int connectionAttempts = 5)
        {
            for (int i = 0; i < connectionAttempts; i++)
            {
                if (await ConnectInternal())
                {
                    return true;
                }
                await Task.Delay(1000);
            }
            return false;
        }

        protected abstract Task<bool> ConnectInternal();
    }
}
