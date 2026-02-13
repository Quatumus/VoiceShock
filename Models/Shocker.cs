using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using VoiceShock.Data;

namespace VoiceShock.Models;

public class Shocker : INotifyPropertyChanged
{
    private bool _isActive;
    
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShockerId { get; set; } = string.Empty;
    
    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (SetProperty(ref _isActive, value))
            {
                // Auto-save state when IsActive changes
                _ = SaveStateAsync();
            }
        }
    }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastActivatedAt { get; set; }
    
    public override string ToString()
    {
        return $"{Name} ({ShockerId})";
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
    
    private async Task SaveStateAsync()
    {
        try
        {
            using var db = new AppDbContext();
            var setting = db.Settings.FirstOrDefault(s => s.Key == $"ShockerState_{ShockerId}");
            
            System.Console.WriteLine($"[DEBUG_LOG] Auto-saving state for shocker {ShockerId}: {IsActive}");
            
            if (setting == null)
            {
                setting = new Setting 
                { 
                    Key = $"ShockerState_{ShockerId}", 
                    Value = IsActive.ToString() 
                };
                db.Settings.Add(setting);
                System.Console.WriteLine($"[DEBUG_LOG] Created new setting: {setting.Key} = {setting.Value}");
            }
            else
            {
                setting.Value = IsActive.ToString();
                System.Console.WriteLine($"[DEBUG_LOG] Updated existing setting: {setting.Key} = {setting.Value}");
            }
            
            var changes = await db.SaveChangesAsync();
            System.Console.WriteLine($"[DEBUG_LOG] Database changes saved: {changes} rows affected");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[DEBUG_LOG] Error auto-saving shocker state: {ex.Message}");
        }
    }
}
