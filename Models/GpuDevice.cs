namespace VoiceShock.Models;

public class GpuDevice
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "Integrated", "Discrete", "CPU"
    public int MemoryMB { get; set; }
    public string Vendor { get; set; } = string.Empty; // "AMD", "NVIDIA", "Intel"
    public bool IsAvailable { get; set; }
    
    public override string ToString()
    {
        return $"{Name} ({Type}, {MemoryMB}MB)";
    }
}
