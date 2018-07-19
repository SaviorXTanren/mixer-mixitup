using MixItUp.Base.Services;
using MixItUp.Base.Util;
using StreamDeckSharp;
using System;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
    public class StreamDeckService : IStreamDeckService
    {
        public event EventHandler<bool> ConnectionStateChangeOccurred;
        public event EventHandler<StreamDeckKeyEvent> KeyChangeOccurred;

        private string deviceName;

        private IStreamDeck deck;

        public StreamDeckService() { }

        public StreamDeckService(string deviceName)
        {
            this.deviceName = deviceName;
        }

        public async Task<bool> Connect()
        {
            bool result = false;
            try
            {
                this.deck = StreamDeck.OpenDevice();
                await this.ConnectionWrapper((deck) =>
                {
                    deck.ConnectionStateChanged -= Deck_ConnectionStateChanged;
                    deck.KeyStateChanged -= Deck_KeyStateChanged;

                    deck.ShowLogo();
                    deck.ConnectionStateChanged += Deck_ConnectionStateChanged;
                    deck.KeyStateChanged += Deck_KeyStateChanged;

                    result = true;

                    return Task.FromResult(0);
                });
            }
            catch (Exception ex) { Logger.Log(ex); }
            return result;
        }

        public async Task Disconnect()
        {
            await this.ConnectionWrapper((deck) =>
            {
                deck.ClearKeys();
                deck.ConnectionStateChanged -= Deck_ConnectionStateChanged;
                deck.KeyStateChanged -= Deck_KeyStateChanged;
                return Task.FromResult(0);
            });

            this.deck.Dispose();
            this.deck = null;
        }

        public async Task SetBrightness(int brightness)
        {
            await this.ConnectionWrapper((deck) =>
            {
                deck.SetBrightness((byte)MathHelper.Clamp(brightness, 0, 100));
                return Task.FromResult(0);
            });
        }

        public async Task ShowColor(int keyID, byte r, byte g, byte b)
        {
            await this.ConnectionWrapper((deck) =>
            {
                KeyBitmap bitmap = KeyBitmap.FromRGBColor(r, g, b);
                deck.SetKeyBitmap(keyID, bitmap);
                return Task.FromResult(0);
            });
        }

        public async Task ShowImage(int keyID, string imageFilePath)
        {
            await this.ConnectionWrapper((deck) =>
            {
                KeyBitmap bitmap = KeyBitmap.FromFile(imageFilePath);
                deck.SetKeyBitmap(keyID, bitmap);
                return Task.FromResult(0);
            });
        }

        public async Task ClearKey(int keyID)
        {
            await this.ConnectionWrapper((deck) =>
            {
                deck.ClearKey(keyID);
                return Task.FromResult(0);
            });
        }

        public async Task ClearAllKeys()
        {
            await this.ConnectionWrapper((deck) =>
            {
                deck.ClearKeys();
                return Task.FromResult(0);
            });
        }

        public async Task<int> GetKeyCount()
        {
            int result = 0;
            await this.ConnectionWrapper((deck) =>
            {
                result = deck.KeyCount;
                return Task.FromResult(0);
            });
            return result;
        }

        public async Task<int> GetIconSize()
        {
            int result = 0;
            await this.ConnectionWrapper((deck) =>
            {
                result = deck.IconSize;
                return Task.FromResult(0);
            });
            return result;
        }

        private async Task ConnectionWrapper(Func<IStreamDeck, Task> action)
        {
            try
            {
                if (this.deck != null && deck.IsConnected)
                {
                    await action(deck);
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        private void Deck_ConnectionStateChanged(object sender, ConnectionEventArgs e)
        {
            if (this.ConnectionStateChangeOccurred != null)
            {
                this.ConnectionStateChangeOccurred(this, e.NewConnectionState);
            }
        }

        private void Deck_KeyStateChanged(object sender, KeyEventArgs e)
        {
            if (this.KeyChangeOccurred != null)
            {
                this.KeyChangeOccurred(this, new StreamDeckKeyEvent(e.Key, e.IsDown));
            }
        }
    }
}
