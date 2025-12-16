using PianoTrainerApp.Services;
using PianoTrainerApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PianoTrainerApp.Models;


namespace PianoTrainerApp.Views
{
    /// <summary>
    /// Логика взаимодействия для PianoWindow.xaml
    /// </summary>
    public partial class PianoWindow : Window
    {
        private PianoViewModel pianoVM;
        private double pixelsPerSecond = 100; // масштаб падения

        private PitchDetector detector;

        private bool isPaused = false;
        private DateTime pauseStartTime;
        private bool pauseHandled = false;

        private static readonly string[] NoteNames =
        {
            "C", "C#", "D", "D#", "E", "F",
            "F#", "G", "G#", "A", "A#", "B"
        };

        public bool IsInitialized { get; private set; }


        public PianoWindow(Song song, double speedMultiplier = 1.0)
        {
            InitializeComponent();
            pianoVM = new PianoViewModel
            {
                SpeedMultiplier = speedMultiplier
            };
            DataContext = pianoVM;

            try
            {
                var midiPath = song.MidiPath;
                if (!System.IO.Path.IsPathRooted(midiPath))
                    midiPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, midiPath);

                var notes = MidiParser.ParseMidi(midiPath);
                // -------------------------
                // НОРМАЛИЗАЦИЯ ВРЕМЕНИ (starttime делаем у всех 0, и, соотвественно изменяем время у нот) (т.к. в midi - либо сразу начинается мелодия, либо нет)
                // -------------------------
                double minStart = notes.Min(n => n.StartTime);

                if (Math.Abs(minStart) > 0.0001) // если первая нота НЕ 0
                {
                    foreach (var n in notes)
                        n.StartTime -= minStart;
                }

                pianoVM.StartAnimation(notes);

                CompositionTarget.Rendering += UpdateNotes;

                // Подключаем микрофон для подсветки клавиш
                detector = new PitchDetector();
                detector.OnNotesDetected += OnNotesDetected;
                detector.Start();

                IsInitialized = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
                IsInitialized = false;
            }
        }

        private void UpdateNotes(object sender, EventArgs e)
        {
            // Останавливаем падение нот
            if (isPaused)
                return;

            NotesCanvas.Children.Clear();

            // Сначала - отрисовка вертикальных линий (октав)
            DrawOctaveLines();

            // допуск для группировки нот с одинаковым стартом
            var epsilon = 0.001;

            // группируем ноты по стартовому времени
            var grouped = pianoVM.FallingNotes
                .GroupBy(n => Math.Round(n.StartTime / epsilon) * epsilon)
                .OrderBy(g => g.Key);

            foreach (var group in grouped)
            {
                foreach (var note in group)
                {
                    if (note.HasCompleted)
                        continue; // уже сыграна нота — пропускаем полностью

                    double delta = pianoVM.CurrentTime - note.StartTime;

                    // если нота ещё не должна появляться, пропускаем
                    if (delta < 0)
                        continue;

                    //double noteWidth = 30;
                    //double noteHeight = note.Duration * pixelsPerSecond;

                    // Определяем ширину ноты: чёрная (с #) уже, белая — шире
                    double noteWidth = note.NoteName.Contains("#") ? 25 : 40;
                    double noteHeight = note.Duration * pixelsPerSecond;

                    // плавное падение сверху: верхняя граница ноты стартует за экраном
                    //double y = delta * pixelsPerSecond - noteHeight;

                    // Чтобы не было нажатия на клавиши изначально
                    double startOffset = 100; // пиксели сверху, нота появляется за экраном

                    double y = delta * pixelsPerSecond - noteHeight - startOffset;


                    // ------------------------------------------------------
                    // Проверка: нота касается клавиатуры
                    // ------------------------------------------------------
                    double noteBottom = y + noteHeight;
                    double noteTop = y;

                    // верх клавиатуры = вся высота NotesCanvas
                    double keyboardTopY = NotesCanvas.RenderSize.Height;  // видимая высота элемента в текущем layout,
                                                                          // а не его растянутая высота в ScrollViewer

                    // касание клавиатуры
                    if (!note.HasPressed && noteBottom >= keyboardTopY)
                    {
                        if (!isPaused)
                        {
                            // текущие ноты на клавиатуре
                            var notesTouchingKeyboard = group
                                .Where(n => !n.HasPressed && !n.HasCompleted && (delta * pixelsPerSecond - noteHeight - startOffset + noteHeight) >= keyboardTopY)
                                .ToList();

                            if (notesTouchingKeyboard.Any())
                            {
                                isPaused = true;
                                pauseHandled = false;
                                pauseStartTime = DateTime.Now;
                                pianoVM.WaitingChord = notesTouchingKeyboard;

                                foreach (var n in notesTouchingKeyboard)
                                {
                                    pianoVM.PressKey(n.NoteName);
                                    n.HasPressed = true;
                                }
                            }
                        }
                    }

                    // снимаем нажатие, если нота ушла
                    if (note.HasPressed && noteTop > keyboardTopY)
                    {
                        pianoVM.ReleaseKey(note.NoteName);
                        note.HasPressed = false;
                    }

                    var rect = new System.Windows.Shapes.Rectangle
                    {
                        Width = noteWidth,
                        Height = noteHeight,
                        Fill = (Brush)new BrushConverter().ConvertFromString("#00E5FF"),

                        // Обводка
                        Stroke = (Brush)new BrushConverter().ConvertFromString("#76E5F2"),
                        StrokeThickness = 1,

                        // Скругление краёв
                        RadiusX = 2,
                        RadiusY = 2
                    };

                    Canvas.SetLeft(rect, note.X);
                    Canvas.SetTop(rect, y);
                    NotesCanvas.Children.Add(rect);

                    var text = new TextBlock
                    {
                        Text = note.NoteName,
                        Foreground = Brushes.White,
                        FontSize = 10,
                        FontWeight = FontWeights.Bold,
                        Width = noteWidth,
                        Height = noteHeight,
                        TextAlignment = TextAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    Canvas.SetLeft(text, note.X);
                    Canvas.SetTop(text, y + (noteHeight - text.FontSize) / 4);
                    NotesCanvas.Children.Add(text);


                }
            }
        }

        // Вертикальные линии - октавы
        private void DrawOctaveLines()
        {
            // Удаляем старые линии
            for (int i = NotesCanvas.Children.Count - 1; i >= 0; i--)
            {
                if (NotesCanvas.Children[i] is System.Windows.Shapes.Line line && line.Tag?.ToString() == "OctaveLine")
                {
                    NotesCanvas.Children.RemoveAt(i);
                }
            }

            double canvasHeight = NotesCanvas.ActualHeight; // Берём реальную видимую высоту

            if (canvasHeight <= 0) return; // Canvas ещё не отрисован, отложим

            foreach (var key in pianoVM.WhiteKeys)
            {
                if (key.Note.StartsWith("C")) // линии на ноте C
                {
                    var line = new System.Windows.Shapes.Line
                    {
                        X1 = key.PositionX,
                        Y1 = 0,
                        X2 = key.PositionX,
                        Y2 = canvasHeight,
                        Stroke = Brushes.Gray,
                        StrokeThickness = 1,
                        StrokeDashArray = new DoubleCollection { 4, 4 },
                        Tag = "OctaveLine"
                    };
                    NotesCanvas.Children.Add(line);
                }
            }
        }

        // Метод получения нот с микрофона
        private void OnNotesDetected(List<string> notes)
        {
            Dispatcher.Invoke(() =>
            {
                // красим ТОЛЬКО текущие клавиши
                foreach (var key in pianoVM.WhiteKeys.Concat(pianoVM.BlackKeys))
                    key.IsCorrectlyPlayed = null;

                foreach (var n in pianoVM.WaitingChord)
                {
                    var key = pianoVM.WhiteKeys.Concat(pianoVM.BlackKeys)
                                .FirstOrDefault(k => k.Note == n.NoteName);
                    if (key != null)
                        key.IsCorrectlyPlayed = notes.Contains(n.NoteName);
                }

                // снимаем сыгранные ноты
                foreach (var n in pianoVM.WaitingChord
                             .Where(n => notes.Contains(n.NoteName))
                             .ToList())
                {
                    n.HasCompleted = true;
                    n.HasPressed = false;
                    pianoVM.ReleaseKey(n.NoteName);
                    pianoVM.WaitingChord.Remove(n);
                }

                // 🔥 КЛЮЧЕВОЕ ИЗМЕНЕНИЕ
                if (!pianoVM.WaitingChord.Any() && isPaused && !pauseHandled)
                {
                    pauseHandled = true; // ❗ срабатывает ТОЛЬКО ОДИН РАЗ
                    TimeSpan pauseDuration = DateTime.Now - pauseStartTime;
                    pianoVM.AdjustStartTimeForPause(pauseDuration);
                    isPaused = false;
                }
            });
        }



        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            detector?.Stop();
        }



        /*public static int MidiNoteNumber(string note)
        {
            string[] names = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
            string namePart = note.Substring(0, note.Length - 1);
            int octave = int.Parse(note.Substring(note.Length - 1, 1));
            int index = Array.IndexOf(names, namePart);
            return index + (octave + 1) * 12;
        }*/

    }
}

