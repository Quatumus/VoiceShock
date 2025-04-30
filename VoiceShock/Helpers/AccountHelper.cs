using System;
using Microsoft.Data.Sqlite;
using VoiceShock.Config;
using System.Diagnostics;
using System.Threading.Tasks;

namespace VoiceShock.Helpers
{
    static class AccountHelper
    {
        private static bool _hasBeenInitialized = false;

        private static async Task Init()
        {
            if (_hasBeenInitialized)
                return;

            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            try
            {
                var createTableCmd = connection.CreateCommand();
                createTableCmd.CommandText = @" 
                CREATE TABLE IF NOT EXISTS Account (
                Backend TEXT PRIMARY KEY,
                Password TEXT,
                Email TEXT,
                Token TEXT
                );";
                await createTableCmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
                throw;
            }

            _hasBeenInitialized = true;
        }

        public static async Task<bool> LoadAsync()
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM Account";

            await using var reader = await selectCmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                AccountConfig.Token = reader.GetString(0);
                AccountConfig.Password = reader.GetString(1);
                AccountConfig.Email = reader.GetString(2);
                AccountConfig.Backend = new Uri(reader.GetString(3));
                return true;
            }
            else
            {
                return false;
            }

        }

        public static async Task<int> SaveItemAsync()
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            // delete old data
            var deleteCmd = connection.CreateCommand();
            deleteCmd.CommandText = "DELETE FROM Account";
            await deleteCmd.ExecuteNonQueryAsync();

            // save new data
            var saveCmd = connection.CreateCommand();
            saveCmd.CommandText = @"
            INSERT OR REPLACE INTO Account (Backend, Password, Email, Token)
            VALUES (@Backend, @Password, @Email, @Token);";

            saveCmd.Parameters.AddWithValue("@Token", AccountConfig.Token);
            saveCmd.Parameters.AddWithValue("@Password", AccountConfig.Password);
            saveCmd.Parameters.AddWithValue("@Email", AccountConfig.Email);
            saveCmd.Parameters.AddWithValue("@Backend", AccountConfig.Backend?.ToString());

            return await saveCmd.ExecuteNonQueryAsync();
        }

        public static async Task<int> DeleteItemAsync(string item)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var deleteCmd = connection.CreateCommand();
            deleteCmd.CommandText = "DELETE FROM Account WHERE Backend = @item";
            deleteCmd.Parameters.AddWithValue("@item", item);

            return await deleteCmd.ExecuteNonQueryAsync();
        }

        public static async Task DropTableAsync()
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var dropTableCmd = connection.CreateCommand();
            dropTableCmd.CommandText = "DROP TABLE IF EXISTS Account";

            await dropTableCmd.ExecuteNonQueryAsync();
            _hasBeenInitialized = false;
        }
    }
}
