using System.Collections.Generic;
using VoiceShock.Config;
using System.ComponentModel;

public enum ControlType
{
    Stop = 0,
    Shock = 1,
    Vibrate = 2,
    Sound = 3
}

namespace VoiceShock.Config
{
    public class Word
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public List<WordShocker> WordShockers { get; set; }
    }

    public class Shocker
    {
        public int Id { get; set; }
        public string ShockerID { get; set; }
        public bool Enabled { get; set; }
        public List<WordShocker> WordShockers { get; set; }
    }

    public class WordShocker
    {
        public int WordId { get; set; }
        public Word Word { get; set; }

        public int ShockerId { get; set; }
        public Shocker Shocker { get; set; }

        public int Duration { get; set; }
        public int Intensity { get; set; }
        public ControlType ControlType { get; set; }
        public ControlType Warning { get; set; }
    }

}
