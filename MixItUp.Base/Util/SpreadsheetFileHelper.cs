using MixItUp.Base.Services;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MixItUp.Base.Util
{
    public class SpreadsheetFileHelper
    {
        public const string ColumnSeparator = "\t";

        public static async Task ExportToCSV(string filePath, IEnumerable<IEnumerable<string>> contents)
        {
            StringBuilder fileContents = new StringBuilder();
            foreach (IEnumerable<string> line in contents)
            {
                fileContents.AppendLine(string.Join(ColumnSeparator, line));
            }
            await ServiceManager.Get<IFileService>().SaveFile(filePath, fileContents.ToString());
        }
    }
}
