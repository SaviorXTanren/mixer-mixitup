using MixItUp.Base;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Chat
{
    /// <summary>
    /// Interaction logic for ChatControl.xaml
    /// </summary>
    public partial class ChatControl : UserControl
    {
        public ChatControl()
        {
            InitializeComponent();
        }

        public async Task Initialize()
        {
            await MixerAPIHandler.MixerClient.Channels.GetChannel("MyBoomShtick");

            await MixerAPIHandler.InitializeChatClient()
        }
    }
}
