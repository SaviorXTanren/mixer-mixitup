using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IDatabaseService
    {
        Task Read(string databaseFilePath, string commandString, Action<Dictionary<string, object>> processRow);

        Task Read(string databaseFilePath, string commandString, Dictionary<string, object> parameters, Action<Dictionary<string, object>> processRow);

        Task Write(string databaseFilePath, string commandString);

        Task BulkWrite(string databaseFilePath, string commandString, IEnumerable<Dictionary<string, object>> parameters);

        void ClearAllPools();

        Task CompressDb(string databaseFilePath);
    }

    public static class DbDataReaderExtensions
    {
        public static bool ColumnExists(this DbDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
