using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using VoiceShock.Config;

namespace VoiceShock.Helpers;

public static class DatabaseHelper
{
    private static string _connectionString = "";
    public static string ConnectionString => _connectionString;

    public static void Initialize(string databaseFilePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(databaseFilePath)!);
        _connectionString = $"Data Source={databaseFilePath}";

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Words (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Text TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Shockers (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ShockerID TEXT NOT NULL,
                Enabled BOOLEAN NOT NULL
            );

            CREATE TABLE IF NOT EXISTS WordShockers (
                WordId INTEGER NOT NULL,
                ShockerId INTEGER NOT NULL,
                Duration INTEGER NOT NULL,
                Intensity INTEGER NOT NULL,
                ControlType INTEGER NOT NULL,
                Warning INTEGER NOT NULL,
                PRIMARY KEY (WordId, ShockerId),
                FOREIGN KEY (WordId) REFERENCES Words(Id) ON DELETE CASCADE,
                FOREIGN KEY (ShockerId) REFERENCES Shockers(Id) ON DELETE CASCADE
            );
        ";
        cmd.ExecuteNonQuery();
    }

    public static int AddWord(string text)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "INSERT INTO Words (Text) VALUES ($text); SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("$text", text);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public static int AddShocker(string shockerId, bool enabled)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "INSERT INTO Shockers (ShockerID, Enabled) VALUES ($id, $enabled); SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("$id", shockerId);
        cmd.Parameters.AddWithValue("$enabled", enabled ? 1 : 0);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public static void LinkWordToShocker(int wordId, int shockerId, int duration, int intensity, int controlType, int warning)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT OR REPLACE INTO WordShockers 
            (WordId, ShockerId, Duration, Intensity, ControlType, Warning) 
            VALUES ($wordId, $shockerId, $duration, $intensity, $controlType, $warning);
        ";
        cmd.Parameters.AddWithValue("$wordId", wordId);
        cmd.Parameters.AddWithValue("$shockerId", shockerId);
        cmd.Parameters.AddWithValue("$duration", duration);
        cmd.Parameters.AddWithValue("$intensity", intensity);
        cmd.Parameters.AddWithValue("$controlType", controlType);
        cmd.Parameters.AddWithValue("$warning", warning);
        cmd.ExecuteNonQuery();
    }

    public static List<(string ShockerID, bool Enabled, int Duration, int Intensity, int ControlType, int Warning)> GetShockersForWord(int wordId)
    {
        var result = new List<(string, bool, int, int, int, int)>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT s.ShockerID, s.Enabled, ws.Duration, ws.Intensity, ws.ControlType, ws.Warning
            FROM WordShockers ws
            JOIN Shockers s ON ws.ShockerId = s.Id
            WHERE ws.WordId = $wordId;
        ";
        cmd.Parameters.AddWithValue("$wordId", wordId);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add((
                reader.GetString(0),
                reader.GetBoolean(1),
                reader.GetInt32(2),
                reader.GetInt32(3),
                reader.GetInt32(4),
                reader.GetInt32(5)
            ));
        }

        return result;
    }
}