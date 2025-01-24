using MixItUp.Base.Util;
using System;
using System.Threading.Tasks;

namespace MixItUp.WPF.Util
{
    public static class UIHelpers
    {
        public static async Task CopyToClipboard(string text)
        {
            try
            {
                System.Windows.Clipboard.SetDataObject(text);
                return;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            try
            {
                System.Windows.Forms.Clipboard.SetDataObject(text);
                return;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            await DialogHelper.ShowMessage(MixItUp.Base.Resources.UnableToCopyToClipboard);
        }
    }
}
