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
        private LearningMode _currentMode;
        public LearningMode CurrentMode
        {
            get => _currentMode;
            set
            {
                if (_currentMode != value)
                {
                    _currentMode = value;
                    OnPropertyChanged(nameof(CurrentMode));

                    // Обновляем все зависящие свойства
                    OnPropertyChanged(nameof(IsTheory));
                    OnPropertyChanged(nameof(IsReading));
                    OnPropertyChanged(nameof(IsHearing));
                    OnPropertyChanged(nameof(ShowNote));
                    OnPropertyChanged(nameof(ShowQuestionMark));
                    OnPropertyChanged(nameof(ShowSoundButton));
                    OnPropertyChanged(nameof(ShowNextButton));
                    OnPropertyChanged(nameof(ShowLine));
                }
            }
        }

        // ---------------------------
        // Режимы для XAML
        // ---------------------------
        public bool IsTheory => CurrentMode == LearningMode.Theory;
        public bool IsReading => CurrentMode == LearningMode.Reading;
        public bool IsHearing => CurrentMode == LearningMode.Hearing;

        // ---------------------------
        // Visibility для элементов
        // ---------------------------
        public bool ShowNote => IsTheory || IsReading;               // ноты на нотном стане
        public bool ShowQuestionMark => IsHearing;                  // знак вопроса при слухе
        public bool ShowSoundButton => !IsReading;  // кнопка звука: теория + слух
        public bool ShowNextButton => !IsTheory;  // кнопка далее в челленджах


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

                // Обновляем все зависящие свойства
                OnPropertyChanged(nameof(NoteY));
                OnPropertyChanged(nameof(FlatNoteY));
                OnPropertyChanged(nameof(NoteTextY));
                OnPropertyChanged(nameof(FlatNoteTextY));
                OnPropertyChanged(nameof(ShowLine));
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
        private string _header;
        public string Header
        {
            get => _header;
            set
            {
                _header = value;
                OnPropertyChanged();
            }
        }

        // Фраза рядом с нотным станом
        private string _instruction;
        public string Instruction
        {
            get => _instruction;
            set { _instruction = value; OnPropertyChanged(nameof(Instruction)); }
        }

        // Для отображения горизонтальной линии на ноте
        public bool ShowLine => !IsHearing && (SelectedNote == "C" || SelectedNote == "C#");
        // Y позиция линии на ноте
        public double LineY => NoteY + 6;

        // Выбранный челлендж

        private LearningMode _selectedChallenge;
        public LearningMode SelectedChallenge
        {
            get => _selectedChallenge;
            set
            {
                _selectedChallenge = value;
                OnPropertyChanged(nameof(SelectedChallenge));

                UpdateChallenge();
            }
        }

        
        public LessonsViewModel()
        {
            WhiteKeys = new ObservableCollection<PianoKey>();
            BlackKeys = new ObservableCollection<PianoKey>();

            GenerateKeys();

            CurrentMode = LearningMode.Theory; // по умолчанию теория

            SelectedNote = "C";
            Header = "До 1 октавы";
        }

        public bool IsNoteSelected(string note)
        {
            return SelectedNote == note;
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
            if (IsTheory) // подсветка только в теории
            {
                foreach (var key in WhiteKeys.Concat(BlackKeys))
                    key.IsPressed = key.Note == SelectedNote;
            }
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
                    ShowFlatNote = !IsHearing ? true : false;
                    NotePrefixFlat = "♭";
                    FlatNoteY = 52;
                    break;
                case "D#":
                    NoteY = 52;
                    NotePrefixSharp = "♯";
                    ShowFlatNote = !IsHearing ? true : false;
                    NotePrefixFlat = "♭";
                    FlatNoteY = 46;
                    break;
                case "F#":
                    NoteY = 39;
                    NotePrefixSharp = "♯";
                    ShowFlatNote = !IsHearing ? true : false;
                    NotePrefixFlat = "♭";
                    FlatNoteY = 33;
                    break;
                case "G#":
                    NoteY = 33;
                    NotePrefixSharp = "♯";
                    ShowFlatNote = !IsHearing ? true : false;
                    NotePrefixFlat = "♭";
                    FlatNoteY = 26;
                    break;
                case "A#":
                    NoteY = 26;
                    NotePrefixSharp = "♯";
                    ShowFlatNote = !IsHearing ? true : false;
                    NotePrefixFlat = "♭";
                    FlatNoteY = 20;
                    break;
            }

            // Меняем тексты подсказок
            if (ShowFlatNote)
            {
                Instruction = "Энгармонически равные звуки\n- звучат одинаково, а пишутся по-разному";
            }
            else
            {
                Instruction = "Сыграйте ноту самостоятельно,\nчтобы лучше её почувствовать!";
            }

        }

        // Метод для смены режима
        public void SetMode(LearningMode mode)
        {
            CurrentMode = mode;

            // если это челлендж, обновляем SelectedChallenge
            if (mode == LearningMode.Reading || mode == LearningMode.Hearing)
                SelectedChallenge = mode;

            // для теории SelectedChallenge = null
            if (mode == LearningMode.Theory)
                SelectedChallenge = LearningMode.Theory;
        }

        private void UpdateChallenge()
        {
            if (SelectedChallenge == LearningMode.Reading || SelectedChallenge == LearningMode.Hearing)
            {
                GenerateRandomNoteForChallenge();
            }
        }

        private Random _random = new Random();

        public void GenerateRandomNoteForChallenge()
        {
            string[] notes = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

            // Сбрасываем выделение на пианино и текстах
            foreach (var key in WhiteKeys.Concat(BlackKeys))
                key.IsPressed = false;

            SelectedNote = notes[_random.Next(notes.Length)];

            // Заголовок и текст инструкции
            if (CurrentMode == LearningMode.Reading)
            {
                Header = "Чтение нот";
                if (ShowFlatNote)
                {
                    Instruction = "Определите ноты слева и нажмите\nсоответствующую клавишу ниже";
                }
                else
                {
                    Instruction = "Определите ноту слева и нажмите\nсоответствующую клавишу ниже";
                }
            }
            else if (CurrentMode == LearningMode.Hearing)
            {
                Header = "Музыкальный слух";
                Instruction = "Определи ноту по звуку и нажми\nсоответствующую клавишу ниже";
            }

            OnPropertyChanged(nameof(Header));
            OnPropertyChanged(nameof(Instruction));
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
