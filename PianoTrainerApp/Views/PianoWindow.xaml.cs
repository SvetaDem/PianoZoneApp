using PianoTrainerApp.Models;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace PianoTrainerApp.Views
{
    /// <summary>
    /// Логика взаимодействия для PianoWindow.xaml
    /// </summary>
    public partial class PianoWindow : Window
    {
        private PianoViewModel pianoVM;
        private PlayMode Mode;
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

        private DateTime? waitingChordStartTime = null;
        private readonly TimeSpan chordTimeout = TimeSpan.FromSeconds(10);

        private bool finishHandled = false;

        public PianoWindow(Song song, double speedMultiplier = 1.0, PlayMode mode = PlayMode.Practice)
        {
            InitializeComponent();

            Mode = mode;

            pianoVM = new PianoViewModel
            {
                SpeedMultiplier = speedMultiplier,
                Mode = mode
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
                CustomMessageBox.Show($"Возникла ошибка:\n{ex.Message}", "Ошибка открытия файла", CustomMessageBoxButton.OK, CustomMessageBoxImage.Error);
                IsInitialized = false;
            }
        }

        private bool countdownStarted = false;

        private double startTimeForProgress = -1;
        private double endTimeForProgress = -1;
        private bool progressInitialized = false;
        private void UpdateNotes(object sender, EventArgs e)
        {
            // Останавливаем падение нот
            if (isPaused)
                return;

            NotesCanvas.Children.Clear();

            // Сначала - отрисовка вертикальных линий (октав)
            DrawOctaveLines();

            // ------------------------------------------------------
            // ИНИЦИАЛИЗАЦИЯ ПРОГРЕССА (ОДИН РАЗ)
            // ------------------------------------------------------
            if (!progressInitialized && pianoVM.FallingNotes.Any())
            {
                double keyboardTopY = NotesCanvas.RenderSize.Height;
                double startOffset = 100;

                // защита: если canvas ещё не отрисован
                if (keyboardTopY <= 0)
                    return;

                var firstNote = pianoVM.FallingNotes
                    .OrderBy(n => n.StartTime)
                    .First();

                var lastNote = pianoVM.FallingNotes
                    .OrderByDescending(n => n.StartTime + n.Duration)
                    .First();

                startTimeForProgress = firstNote.StartTime + (keyboardTopY + startOffset) / pixelsPerSecond;

                endTimeForProgress = lastNote.StartTime +
                    (keyboardTopY + startOffset + lastNote.Duration * pixelsPerSecond) / pixelsPerSecond;

                progressInitialized = true;
            }

            // ------------------------------------------------------
            // ОБНОВЛЕНИЕ ПРОГРЕССА
            // ------------------------------------------------------
            if (progressInitialized && endTimeForProgress > startTimeForProgress)
            {
                double progress = (pianoVM.CurrentTime - startTimeForProgress) /
                                  (endTimeForProgress - startTimeForProgress);

                progress = Math.Max(0, Math.Min(progress, 1));

                UpdateProgress(progress);
            }

            // запуск countdown перед первой нотой
            if (!countdownStarted)
            {
                var firstNote = pianoVM.FallingNotes
                                      .Where(n => !n.HasCompleted)
                                      .OrderBy(n => n.StartTime)
                                      .FirstOrDefault();

                if (firstNote != null)
                {
                    countdownStarted = true;
                    _ = StartCountdown(firstNote);
                }
            }

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

                    // В режиме Advanced: если нота уже ушла за клавиатуру — считаем её завершённой
                    if (Mode == PlayMode.Advanced && !note.HasCompleted && noteTop > keyboardTopY)
                    {
                        note.HasCompleted = true;

                        if (!note.IsCounted)
                        {
                            totalNotes++;
                            note.IsCounted = true;
                        }
                    }

                    // касание клавиатуры
                    if (!note.HasPressed && noteBottom >= keyboardTopY)
                    {
                        if (Mode == PlayMode.Practice)
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
                        else if (Mode == PlayMode.Advanced)
                        {
                            // В режиме точности просто фиксируем ноты как "нажатые", но не ставим паузу
                            if (!note.HasPressed)
                            {
                                pianoVM.PressKey(note.NoteName);
                                note.HasPressed = true;
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
                // Проверка заверешния нот
                if (!finishHandled && pianoVM.IsSongFinished)
                {
                    finishHandled = true;
                    OnSongFinished();
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

        // счетчики для точности
        private int totalNotes = 0;
        private int correctNotes = 0;

        // Метод получения нот с микрофона
        private void OnNotesDetected(List<string> notes)
        {
            Dispatcher.Invoke(() =>
            {
                if (Mode == PlayMode.Practice)
                {
                    // если ждём аккорд — фиксируем старт
                    if (pianoVM.WaitingChord.Any() && waitingChordStartTime == null)
                    {
                        waitingChordStartTime = DateTime.Now;
                    }

                    // ПРОВЕРКА ТАЙМАУТА
                    if (pianoVM.WaitingChord.Any()
                        && waitingChordStartTime != null
                        && DateTime.Now - waitingChordStartTime >= chordTimeout)
                    {
                        // ❌ таймаут — считаем аккорд пропущенным
                        SkipWaitingChord();
                        return;
                    }

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

                        // Подсчет точности только в режиме Advanced
                        if (Mode == PlayMode.Advanced)
                        {
                            if (!n.IsCounted)
                            {
                                totalNotes++;
                                correctNotes++;
                                n.IsCounted = true;
                            }
                        }
                    }

                    // Пропущенные ноты считаем неправильными
                    if (Mode == PlayMode.Advanced)
                    {
                        totalNotes += pianoVM.WaitingChord.Count;
                    }

                    // Обновляем текст
                    if (Mode == PlayMode.Advanced && totalNotes > 0)
                    {
                        double accuracy = (double)correctNotes / totalNotes * 100;
                        AccuracyText.Text = $"{accuracy:F0}%";
                    }

                    // без этого не работает :)
                    if (!pianoVM.WaitingChord.Any() && isPaused && !pauseHandled)
                    {
                        pauseHandled = true; // срабатывает ТОЛЬКО ОДИН РАЗ

                        TimeSpan pauseDuration = DateTime.Now - pauseStartTime;
                        pianoVM.AdjustStartTimeForPause(pauseDuration);

                        isPaused = false;
                        waitingChordStartTime = null; // сброс
                    }
                }
                else if (Mode == PlayMode.Advanced)
                {
                    // сброс подсветки
                    foreach (var key in pianoVM.WhiteKeys.Concat(pianoVM.BlackKeys))
                        key.IsCorrectlyPlayed = null;

                    // 🔥 берём активные ноты (которые сейчас на клавиатуре)
                    var activeNotes = pianoVM.FallingNotes
                        .Where(n => !n.HasCompleted && n.HasPressed)
                        .ToList();

                    foreach (var note in activeNotes)
                    {
                        // подсветка клавиши
                        var key = pianoVM.WhiteKeys.Concat(pianoVM.BlackKeys)
                                    .FirstOrDefault(k => k.Note == note.NoteName);

                        if (key != null)
                            key.IsCorrectlyPlayed = notes.Contains(note.NoteName);

                        // считаем только один раз
                        if (note.IsCounted)
                            continue;

                        // 🎯 считаем попытку
                        totalNotes++;

                        if (notes.Contains(note.NoteName))
                        {
                            correctNotes++;
                            note.HasCompleted = true;
                            pianoVM.ReleaseKey(note.NoteName);
                        }

                        note.IsCounted = true;
                    }

                    // 📊 обновляем точность
                    if (totalNotes > 0)
                    {
                        double accuracy = (double)correctNotes / totalNotes * 100;
                        AccuracyText.Text = $"{accuracy:F0}%";
                    }

                }
            });
        }

        private void SkipWaitingChord()
        {
            // считаем ноты пропущенными
            foreach (var n in pianoVM.WaitingChord)
            {
                n.HasCompleted = true;
                n.HasPressed = false;
                pianoVM.ReleaseKey(n.NoteName);
            }

            pianoVM.WaitingChord.Clear();

            if (isPaused && !pauseHandled)
            {
                pauseHandled = true;
                TimeSpan pauseDuration = DateTime.Now - pauseStartTime;
                pianoVM.AdjustStartTimeForPause(pauseDuration);
                isPaused = false;
            }

            waitingChordStartTime = null;
        }

        private async void OnSongFinished()
        {
            // остановка всего
            detector?.Stop();
            CompositionTarget.Rendering -= UpdateNotes;

            // добиваем прогресс до 100%
            UpdateProgress(1.0);

            await Task.Delay(300); // даём анимации завершиться

            Dispatcher.Invoke(() =>
            {
                var resultWindow = new SongFinishedWindow();
                resultWindow.Owner = this;

                if (resultWindow.ShowDialog() == true)
                {
                    // Повторить
                    RestartSong();
                }
                else
                {
                    // Вернуться в библиотеку
                    this.Close();
                }
            });
        }

        private void RestartSong()
        {
            AccuracyPanel.Visibility = Visibility.Collapsed;
            ProgressPanel.Visibility = Visibility.Collapsed;

            progressInitialized = false;
            startTimeForProgress = -1;
            endTimeForProgress = -1;

            countdownStarted = false;

            finishHandled = false;
            pianoVM.Reset();
            pianoVM.StartAnimation(pianoVM.OriginalNotes);

            detector?.Start();
            CompositionTarget.Rendering += UpdateNotes;
        }


        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            detector?.Stop();
        }

        private void UpdateProgress(double progress) // progress от 0 до 1
        {
            double maxWidth = ProgressSlider.Width; // как в XAML
            double targetWidth = maxWidth * progress;

            var anim = new DoubleAnimation
            {
                To = targetWidth,
                Duration = TimeSpan.FromMilliseconds(300), // плавность
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            ProgressFill.BeginAnimation(FrameworkElement.WidthProperty, anim);

            int percent = (int)(progress * 100);
            ProgressText.Text = percent + "%";
        }

        private async Task StartCountdown(MidiNote firstNote)
        {
            // Считаем, через сколько секунд первая нота дойдет до клавиатуры
            double noteHeight = firstNote.Duration * pixelsPerSecond;
            double startOffset = 100; // как в UpdateNotes
            double keyboardTopY = NotesCanvas.RenderSize.Height;

            // время, когда нота коснется клавиатуры относительно её StartTime
            double timeToKeyboard = (keyboardTopY + startOffset) / pixelsPerSecond;

            // сколько ждать до показа countdown (чтобы 3 секунды отсчета)
            double waitBeforeCountdown = timeToKeyboard - 3.0 - (pianoVM.CurrentTime - firstNote.StartTime);

            // Плавное появление подсказки и слайдера с прогрессом прохождения
            HintText.Visibility = Visibility.Visible;
            ProgressPanel.Visibility = Visibility.Visible;

            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(500));
            HintText.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            ProgressPanel.BeginAnimation(UIElement.OpacityProperty, fadeIn);

            // Ждём до момента начала обратного отсчёта
            if (waitBeforeCountdown > 0)
                await Task.Delay((int)(waitBeforeCountdown * 1000));

            // Показываем сам countdown
            CountdownText.Visibility = Visibility.Visible;

            for (int i = 3; i > 0; i--)
            {
                CountdownText.Text = i.ToString();

                // плавное затухание/появление текста
                var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
                CountdownText.BeginAnimation(UIElement.OpacityProperty, fade);

                await Task.Delay(1000);
            }

            // Скрываем countdown и hint
            CountdownText.Visibility = Visibility.Collapsed;
            if (Mode == PlayMode.Advanced) await Task.Delay(3000);

            // Исчезновение подсказки
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(500));
            fadeOut.Completed += (s, e) => HintText.Visibility = Visibility.Collapsed;
            HintText.BeginAnimation(UIElement.OpacityProperty, fadeOut);

            if (Mode == PlayMode.Advanced)
            {
                AccuracyPanel.Visibility = Visibility.Visible;
                AccuracyPanel.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            }
        }
    }
}

