using System.Collections.Generic;
using VoiceShock.Config;
using System.ComponentModel;
using OpenShock.SDK.CSharp.Models;

namespace VoiceShock.Config
{
    public class Word
    {
        public int Id { get; set; }
        public required string Text { get; set; }
        public required List<WordShocker> WordShockers { get; set; }
    }

    public class Shocker
    {
        public required string Id { get; set; }
        public bool Enabled { get; set; }
        public required List<WordShocker> WordShockers { get; set; }
    }

    public class WordShocker
    {
        public int WordId { get; set; }
        public required Word Word { get; set; }

        public int ShockerId { get; set; }
        public required Shocker Shocker { get; set; }

        public int Duration { get; set; }
        public int Intensity { get; set; }
        public ControlType ControlType { get; set; }
        public ControlType Warning { get; set; }
        public bool Enabled { get; set; }
    }

}
