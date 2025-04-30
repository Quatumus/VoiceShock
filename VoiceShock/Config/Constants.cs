using System;
using System.IO;


namespace VoiceShock.Config
{
    public static class Constants
    {
        public const string DatabaseFilename = "VoiceShockData.db3";

        public static string DatabasePath =>
            $"Data Source={Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), DatabaseFilename)}";

    }
}
