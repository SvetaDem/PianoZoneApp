using PianoTrainerApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace PianoTrainerApp.Services
{
    internal class MidiParser
    {
        private static readonly Dictionary<string, string> NoteMap = new Dictionary<string, string>
        {
            {"CSharp", "C#"},
            {"DSharp", "D#"},
            {"FSharp", "F#"},
            {"GSharp", "G#"},
            {"ASharp", "A#"},
            {"C", "C"},
            {"D", "D"},
            {"E", "E"},
            {"F", "F"},
            {"G", "G"},
            {"A", "A"},
            {"B", "B"}
        };
        public static List<MidiNote> ParseMidi(string path)
        {
            var midiFile = MidiFile.Read(path);
            var tempoMap = midiFile.GetTempoMap();
            var notes = midiFile.GetNotes();

            return notes.Select(n => new MidiNote
            {
                // Формируем строку вроде "C4", "D#5" и т.п.
                NoteName = $"{NoteMap[n.NoteName.ToString()]}{n.Octave}",
                StartTime = n.TimeAs<MetricTimeSpan>(tempoMap).TotalSeconds,
                Duration = n.LengthAs<MetricTimeSpan>(tempoMap).TotalSeconds
            }).ToList();
        }
    }
}
