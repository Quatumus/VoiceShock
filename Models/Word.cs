using System.ComponentModel.DataAnnotations;

namespace VoiceShock.Models;

public class Word
{
    [Key]
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public int Duration { get; set; }
    public int Intensity { get; set; }
}
