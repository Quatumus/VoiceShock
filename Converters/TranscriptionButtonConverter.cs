using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace VoiceShock.Converters;

public class TranscriptionButtonConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isTranscribing)
        {
            return isTranscribing ? "Stop Transcription" : "Start Transcription";
        }
        return "Start Transcription";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
