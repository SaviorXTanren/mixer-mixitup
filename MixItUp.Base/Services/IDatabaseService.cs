using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IDatabaseService
    {
        Task Read(string databaseFilePath, string commandString, Action<DbDataReader> processRow);

        Task Write(string databaseFilePath, string commandString);

        Task BulkWrite(string databaseFilePath, string commandString, IEnumerable<Dictionary<string, object>> parameters);
    }
}
