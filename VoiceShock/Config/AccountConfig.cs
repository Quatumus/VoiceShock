using System;


namespace VoiceShock.Config
{
    public static class AccountConfig
    {
        public static Uri Backend { get; set; } = new Uri("https://api.openshock.app");
        public static string? Token { get; set; }
        public static string? Email { get; set; }
        public static string? Password { get; set; }
    }
}
