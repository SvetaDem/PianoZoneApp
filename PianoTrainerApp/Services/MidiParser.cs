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
        public static List<MidiNote> ParseMidi(string path)
        {
            var midiFile = MidiFile.Read(path);
            var tempoMap = midiFile.GetTempoMap();
            var notes = midiFile.GetNotes();

            return notes.Select(n => new MidiNote
            {
                // Формируем строку вроде "C4", "D#5" и т.п.
                NoteName = $"{n.NoteName}{n.Octave}",
                StartTime = n.TimeAs<MetricTimeSpan>(tempoMap).TotalSeconds,
                Duration = n.LengthAs<MetricTimeSpan>(tempoMap).TotalSeconds
            }).ToList();
        }
    }
}
