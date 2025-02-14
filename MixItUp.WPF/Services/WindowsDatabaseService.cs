using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace MixItUp.WPF.Services
{
    public class WindowsDatabaseService : IDatabaseService
    {
        private const int MaxBulkInsertRows = 10000;

        public async Task Read(string databaseFilePath, string commandString, Action<Dictionary<string, object>> processRow) { await this.Read(databaseFilePath, commandString, null, processRow); }

        public async Task Read(string databaseFilePath, string commandString, Dictionary<string, object> parameters, Action<Dictionary<string, object>> processRow)
        {
            string parameterString = parameters != null ? string.Join(" = ", parameters.Select(p => $"{{{p.Key} - {p.Value}}}")) : string.Empty;
            Logger.Log(LogLevel.Debug, string.Format("SQLite Query: {0} - {1}", commandString, parameterString));

            await this.EstablishConnection(databaseFilePath, (connection) =>
            {
                using (SqliteCommand command = new SqliteCommand(commandString, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var kvp in parameters)
                        {
                            if (kvp.Value == null)
                            {
                                command.Parameters.AddWithValue(kvp.Key, DBNull.Value);
                            }
                            else
                            {
                                command.Parameters.AddWithValue(kvp.Key, kvp.Value);
                            }
                        }
                    }

                    using (SqliteDataReader reader = command.ExecuteReader())
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
                return Task.CompletedTask;
            });
        }

        public async Task Write(string databaseFilePath, string commandString)
        {
            Logger.Log(LogLevel.Debug, string.Format("SQLite Query: {0}", commandString));

            await this.EstablishConnection(databaseFilePath, async (connection) =>
            {
                using (SqliteCommand command = new SqliteCommand(commandString, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            });
        }

        public async Task BulkWrite(string databaseFilePath, string commandString, IEnumerable<Dictionary<string, object>> parameters)
        {
            await this.EstablishConnection(databaseFilePath, async (connection) =>
            {
                for (int i = 0; i < parameters.Count(); i += WindowsDatabaseService.MaxBulkInsertRows)
                {
                    var rowsToInsert = parameters.Skip(i).Take(WindowsDatabaseService.MaxBulkInsertRows);

                    using (SqliteTransaction transaction = connection.BeginTransaction())
                    {
                        using (SqliteCommand command = new SqliteCommand(commandString, connection, transaction))
                        {
                            foreach (Dictionary<string, object> rowParameters in rowsToInsert)
                            {
                                try
                                {
                                    foreach (var kvp in rowParameters)
                                    {
                                        if (kvp.Value == null)
                                        {
                                            command.Parameters.AddWithValue(kvp.Key, DBNull.Value);
                                        }
                                        else
                                        {
                                            command.Parameters.AddWithValue(kvp.Key, kvp.Value);
                                        }
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

        public async Task CompressDb(string databaseFilePath)
        {
            await Write(databaseFilePath, "vacuum;");
        }

        public void ClearAllPools()
        {
            try
            {
                SqliteConnection.ClearAllPools();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private async Task EstablishConnection(string databaseFilePath, Func<SqliteConnection, Task> databaseQuery)
        {
            try
            {
                if (File.Exists(databaseFilePath))
                {
                    await Task.Run(async () =>
                    {
                        try
                        {
                            using (SqliteConnection connection = new SqliteConnection("Data Source=" + databaseFilePath))
                            {
                                await connection.OpenAsync();
                                await databaseQuery(connection);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(ex);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
    }
}