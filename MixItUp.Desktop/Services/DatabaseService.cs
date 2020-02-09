using MixItUp.Base.Services;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
    public class DatabaseService : IDatabaseService
    {
        private const int MaxBulkInsertRows = 10000;

        public async Task Read(string databaseFilePath, string commandString, Action<Dictionary<string, object>> processRow)
        {
            await this.EstablishConnection(databaseFilePath, (connection) =>
            {
                using (SQLiteCommand command = new SQLiteCommand(commandString, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        Dictionary<string, object> values = new Dictionary<string, object>();
                        while (reader.Read())
                        {
                            try
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    values[reader.GetName(i)] = reader.GetValue(i);
                                }
                                processRow(values);
                            }
                            catch (Exception ex) { Logger.Log(ex); }
                        }
                        values.Clear();
                    }
                }
                return Task.FromResult(0);
            });
        }

        public async Task Write(string databaseFilePath, string commandString)
        {
            Logger.Log(LogLevel.Debug, string.Format("SQLite Query: {0}", commandString));

            await this.EstablishConnection(databaseFilePath, async (connection) =>
            {
                using (SQLiteCommand command = new SQLiteCommand(commandString, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            });
        }

        public async Task BulkWrite(string databaseFilePath, string commandString, IEnumerable<Dictionary<string, object>> parameters)
        {
            await this.EstablishConnection(databaseFilePath, async (connection) =>
            {
                for (int i = 0; i < parameters.Count(); i += DatabaseService.MaxBulkInsertRows)
                {
                    var rowsToInsert = parameters.Skip(i).Take(DatabaseService.MaxBulkInsertRows);

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        using (SQLiteCommand command = new SQLiteCommand(commandString, connection))
                        {
                            foreach (Dictionary<string, object> rowParameters in rowsToInsert)
                            {
                                try
                                {
                                    foreach (var kvp in rowParameters)
                                    {
                                        command.Parameters.Add(new SQLiteParameter(kvp.Key, value: kvp.Value));
                                    }

                                    Logger.Log(LogLevel.Debug, string.Format("SQLite Query: {0} - {1}", commandString, JSONSerializerHelper.SerializeToString(rowParameters)));

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

        private async Task EstablishConnection(string databaseFilePath, Func<SQLiteConnection, Task> databaseQuery)
        {
            try
            {
                if (File.Exists(databaseFilePath))
                {
                    using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + databaseFilePath))
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
        }
    }
}