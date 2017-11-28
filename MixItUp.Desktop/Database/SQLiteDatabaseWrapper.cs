using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Database
{
    public class SQLiteDatabaseWrapper
    {
        private const int MaxBulkInsertRows = 10000;

        public string DatabaseFilePath { get; set; }

        public SQLiteDatabaseWrapper(string databaseFilePath)
        {
            this.DatabaseFilePath = databaseFilePath;
        }

        public async Task EstablishConnection(Func<SQLiteConnection, Task> databaseQuery)
        {
            if (File.Exists(this.DatabaseFilePath))
            {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + this.DatabaseFilePath))
                {
                    await connection.OpenAsync();
                    await databaseQuery(connection);
                }
            }
        }

        public async Task ReadRows(string commandString, Action<DbDataReader> processRow)
        {
            await this.EstablishConnection(async (connection) =>
            {
                using (SQLiteCommand command = new SQLiteCommand(commandString, connection))
                {
                    using (DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                processRow(reader);
                            }
                            catch (Exception ex) { Logger.Log(ex); }
                        }
                    }
                }
            });
        }

        public async Task Insert(string commandString, IEnumerable<object> parameters)
        {
            await this.EstablishConnection(async (connection) =>
            {
                using (SQLiteCommand command = new SQLiteCommand(commandString, connection))
                {
                    foreach (object parameter in parameters)
                    {
                        command.Parameters.Add(parameter);
                    }
                    await command.ExecuteNonQueryAsync();
                }
            });
        }

        public async Task BulkInsert(string commandString, IEnumerable<IEnumerable<object>> parameters)
        {

            await this.EstablishConnection(async (connection) =>
            {
                for (int i = 0; i < (parameters.Count() / SQLiteDatabaseWrapper.MaxBulkInsertRows); i++)
                {
                    var rowsToInsert = parameters.Skip(i * SQLiteDatabaseWrapper.MaxBulkInsertRows).Take(SQLiteDatabaseWrapper.MaxBulkInsertRows);

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        using (SQLiteCommand command = new SQLiteCommand(commandString, connection))
                        {
                            foreach (IEnumerable<object> rowParameters in rowsToInsert)
                            {
                                foreach (object rowParam in rowParameters)
                                {
                                    command.Parameters.Add(rowParam);
                                }
                                await command.ExecuteNonQueryAsync();
                                command.Parameters.Clear();
                            }
                        }
                        transaction.Commit();
                    }
                }
            });
        }
    }
}
