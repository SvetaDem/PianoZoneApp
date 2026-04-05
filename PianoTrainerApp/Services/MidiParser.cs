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
    /// <summary>
    /// Сервис для парсинга MIDI-файлов.
    /// Преобразует MIDI-ноты в список объектов MidiNote,
    /// пригодных для использования в приложении (например, для анимации).
    /// </summary>
    internal class MidiParser
    {
        // ---------------------------
        // Сопоставление названий нот
        // ---------------------------
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

        /// <summary>
        /// Читает MIDI-файл и преобразует его в список нот.
        /// </summary>
        public static List<MidiNote> ParseMidi(string path)
        {
            var midiFile = MidiFile.Read(path);
            var tempoMap = midiFile.GetTempoMap();
            var notes = midiFile.GetNotes();

            return notes.Select(n => new MidiNote
            {
                // Формируем строку вроде "C4", "D#5" и т.п.
                NoteName = $"{NoteMap[n.NoteName.ToString()]}{n.Octave}",

                // Время начала ноты (в секундах)
                StartTime = n.TimeAs<MetricTimeSpan>(tempoMap).TotalSeconds,

                // Длительность ноты (в секундах)
                Duration = n.LengthAs<MetricTimeSpan>(tempoMap).TotalSeconds
            }).ToList();
        }
    }
}
