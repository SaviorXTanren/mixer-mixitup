using System;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public class StreamDeckKeyEvent
    {
        public int KeyID { get; set; }
        public bool IsDown { get; set; }

        public StreamDeckKeyEvent(int keyID, bool isDown)
        {
            this.KeyID = keyID;
            this.IsDown = isDown;
        }
    }

    public interface IStreamDeckService
    {
        event EventHandler<bool> ConnectionStateChangeOccurred;
        event EventHandler<StreamDeckKeyEvent> KeyChangeOccurred;

        Task<bool> Connect();
        Task Disconnect();

        Task SetBrightness(int brightness);

        Task ShowColor(int keyID, byte r, byte g, byte b);
        Task ShowImage(int keyID, string imageFilePath);

        Task ClearKey(int keyID);
        Task ClearAllKeys();

        Task<int> GetKeyCount();
        Task<int> GetIconSize();
    }
}
