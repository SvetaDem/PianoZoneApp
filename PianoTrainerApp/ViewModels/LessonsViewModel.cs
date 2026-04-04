using PianoTrainerApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace PianoTrainerApp.ViewModels
{
    public class LessonsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
 
        public ObservableCollection<PianoKey> WhiteKeys { get; set; }
        public ObservableCollection<PianoKey> BlackKeys { get; set; }

        // Выбранная нота
        private string _selectedNote;
        public string SelectedNote
        {
            get => _selectedNote;
            set
            {
                _selectedNote = value;
                OnPropertyChanged(nameof(SelectedNote));

                UpdatePiano();
                UpdateNotePosition();
            }
        }

        // Символ для диеза (справа или слева от круга)
        private string _notePrefixSharp;
        public string NotePrefixSharp
        {
            get => _notePrefixSharp;
            set { _notePrefixSharp = value; OnPropertyChanged(nameof(NotePrefixSharp)); }
        }

        // Символ для бемоля (справа от круга)
        private string _notePrefixFlat;
        public string NotePrefixFlat
        {
            get => _notePrefixFlat;
            set { _notePrefixFlat = value; OnPropertyChanged(nameof(NotePrefixFlat)); }
        }

        // Показывать ли энгармоническую бемоль-ноту
        private bool _showFlatNote;
        public bool ShowFlatNote
        {
            get => _showFlatNote;
            set { _showFlatNote = value; OnPropertyChanged(nameof(ShowFlatNote)); }
        }

        // Y позиция основной ноты
        private double _noteY;
        public double NoteY
        {
            get => _noteY;
            set { _noteY = value; OnPropertyChanged(nameof(NoteY)); }
        }

        // Y позиция диез
        public double NoteTextY => NoteY - 8;

        // Y позиция бемоль-ноты
        private double _flatNoteY;
        public double FlatNoteY
        {
            get => _flatNoteY;
            set { _flatNoteY = value; OnPropertyChanged(nameof(FlatNoteY)); }
        }

        // Y позиция бемоль
        public double FlatNoteTextY => FlatNoteY - 12;

        // Заголовок ноты для отображения справа
        private string _noteHeader;
        public string NoteHeader
        {
            get => _noteHeader;
            set
            {
                _noteHeader = value;
                OnPropertyChanged();
            }
        }

        // Фраза рядом с нотным станом
        private string _noteInstruction;
        public string NoteInstruction
        {
            get => _noteInstruction;
            set { _noteInstruction = value; OnPropertyChanged(nameof(NoteInstruction)); }
        }

        // Для отображения горизонтальной линии на ноте
        private bool _showLine;
        public bool ShowLine
        {
            get => _showLine;
            set { _showLine = value; OnPropertyChanged(nameof(ShowLine)); }
        }
        // Y позиция линии на ноте
        public double LineY => NoteY + 6;

        public bool IsNoteSelected(string note)
        {
            return SelectedNote == note;
        }
        public LessonsViewModel()
        {
            WhiteKeys = new ObservableCollection<PianoKey>();
            BlackKeys = new ObservableCollection<PianoKey>();

            GenerateKeys();

            SelectedNote = "C";
            NoteHeader = "До 1 октавы";
        }

        private void GenerateKeys()
        {
            WhiteKeys.Clear();
            BlackKeys.Clear();

            string[] whiteNotes = { "C", "D", "E", "F", "G", "A", "B" };
            string[] blackNotes = { "C#", "D#", "", "F#", "G#", "A#", "", };

            double x = 0;

            for (int i = 0; i < whiteNotes.Length; i++)
            {
                WhiteKeys.Add(new PianoKey
                {
                    Note = whiteNotes[i],
                    IsBlack = false,
                    PositionX = x
                });

                if (!string.IsNullOrEmpty(blackNotes[i]))
                {
                    BlackKeys.Add(new PianoKey
                    {
                        Note = blackNotes[i],
                        IsBlack = true,
                        PositionX = x + 28
                    });
                }

                x += 40;
            }
        }

        public void SelectNote(string note)
        {
            foreach (var key in WhiteKeys.Concat(BlackKeys))
                key.IsPressed = key.Note == note;
        }

        // Метод обновления пианино
        private void UpdatePiano()
        {
            foreach (var key in WhiteKeys.Concat(BlackKeys))
                key.IsPressed = key.Note == SelectedNote;
        }

        // Метод обновления позиции ноты
        private void UpdateNotePosition()
        {
            // Сбрасываем по умолчанию
            NotePrefixSharp = "";
            NotePrefixFlat = "";
            ShowFlatNote = false;

            switch (SelectedNote)
            {
                // Натуральные ноты
                case "C": NoteY = 60; break;
                case "D": NoteY = 52; break;
                case "E": NoteY = 46; break;
                case "F": NoteY = 39; break;
                case "G": NoteY = 33; break;
                case "A": NoteY = 26; break;
                case "B": NoteY = 20; break;

                // Диезы (слева #) и показываем бемоль-эквивалент
                case "C#":
                    NoteY = 60;
                    NotePrefixSharp = "♯";
                    ShowFlatNote = true;
                    NotePrefixFlat = "♭";
                    FlatNoteY = 52;
                    break;
                case "D#":
                    NoteY = 52;
                    NotePrefixSharp = "♯";
                    ShowFlatNote = true;
                    NotePrefixFlat = "♭";
                    FlatNoteY = 46;
                    break;
                case "F#":
                    NoteY = 39;
                    NotePrefixSharp = "♯";
                    ShowFlatNote = true;
                    NotePrefixFlat = "♭";
                    FlatNoteY = 33;
                    break;
                case "G#":
                    NoteY = 33;
                    NotePrefixSharp = "♯";
                    ShowFlatNote = true;
                    NotePrefixFlat = "♭";
                    FlatNoteY = 26;
                    break;
                case "A#":
                    NoteY = 26;
                    NotePrefixSharp = "♯";
                    ShowFlatNote = true;
                    NotePrefixFlat = "♭";
                    FlatNoteY = 20;
                    break;
            }

            // Меняем тексты подсказок
            if (ShowFlatNote)
            {
                NoteInstruction = "Энгармонически равные звуки\n- звучат одинаково, а пишутся по-разному";
            }
            else
            {
                NoteInstruction = "Сыграйте ноту самостоятельно,\nчтобы лучше её почувствовать!";
            }

            ShowLine = SelectedNote == "C" || SelectedNote == "C#";

            // Уведомляем UI о всех свойствах сразу
            OnPropertyChanged(nameof(NoteY));
            OnPropertyChanged(nameof(FlatNoteY));
            OnPropertyChanged(nameof(NotePrefixSharp));
            OnPropertyChanged(nameof(NotePrefixFlat));
            OnPropertyChanged(nameof(ShowFlatNote));
            OnPropertyChanged(nameof(NoteTextY));
            OnPropertyChanged(nameof(FlatNoteTextY));
        }
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
