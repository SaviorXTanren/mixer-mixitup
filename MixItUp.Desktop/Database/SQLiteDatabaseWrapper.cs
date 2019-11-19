using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
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
            await this.AsyncWrapper(async () =>
            {
                try
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
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            });
        }

        public async Task RunReadCommand(string commandString, Action<SQLiteDataReader> processRow)
        {
            await this.EstablishConnection((connection) =>
            {
                using (SQLiteCommand command = new SQLiteCommand(commandString, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
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
                return Task.FromResult(0);
            });
        }

        public async Task RunWriteCommand(string commandString)
        {
            Logger.Log(LogLevel.Debug, string.Format("SQLite Query: {0}", commandString));

            await this.EstablishConnection(async (connection) =>
            {
                using (SQLiteCommand command = new SQLiteCommand(commandString, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            });
        }

        public async Task RunBulkWriteCommand(string commandString, IEnumerable<IEnumerable<SQLiteParameter>> parameters)
        {
            await this.EstablishConnection(async (connection) =>
            {
                for (int i = 0; i < parameters.Count(); i += SQLiteDatabaseWrapper.MaxBulkInsertRows)
                {
                    var rowsToInsert = parameters.Skip(i).Take(SQLiteDatabaseWrapper.MaxBulkInsertRows);

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        using (SQLiteCommand command = new SQLiteCommand(commandString, connection))
                        {
                            foreach (IEnumerable<SQLiteParameter> rowParameters in rowsToInsert)
                            {
                                try
                                {
                                    foreach (SQLiteParameter rowParam in rowParameters)
                                    {
                                        command.Parameters.Add(rowParam);
                                    }

                                    Logger.Log(LogLevel.Debug, string.Format("SQLite Query: {0} - {1}", commandString, SerializerHelper.SerializeToString(rowParameters)));

                                    await command.ExecuteNonQueryAsync();
                                    command.Parameters.Clear();
                                }
                                catch (Exception ex) { Logger.Log(ex); }
                            }
                        }
                        transaction.Commit();
                    }
                }
            });
        }

        private async Task AsyncWrapper(Func<Task> function) { await Task.Run(async () => { await function(); }); }
    }
}
