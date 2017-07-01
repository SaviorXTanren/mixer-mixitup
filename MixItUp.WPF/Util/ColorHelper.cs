using Mixer.Base.ViewModel.Chat;
using System.Windows.Media;

namespace MixItUp.WPF.Util
{
    public static class ColorHelper
    {
        public static SolidColorBrush GetColorForUser(ChatUserViewModel user)
        {
            switch (user.PrimaryRole)
            {
                case UserRole.Streamer:
                case UserRole.Mod:
                    return Brushes.Green;
                case UserRole.Staff:
                    return Brushes.Gold;
                case UserRole.Subscriber:
                case UserRole.Pro:
                    return Brushes.Purple;
                case UserRole.Banned:
                    return Brushes.Red;
                default:
                    return Brushes.Blue;
            }
        }
    }
}
