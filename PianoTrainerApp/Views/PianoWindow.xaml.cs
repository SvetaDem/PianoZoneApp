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
                pianoVM.StartAnimation(notes);

                CompositionTarget.Rendering += UpdateNotes;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void UpdateNotes(object sender, EventArgs e)
        {
            NotesCanvas.Children.Clear();

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
                    double delta = pianoVM.CurrentTime - note.StartTime;

                    // если нота ещё не должна появляться, пропускаем
                    if (delta < 0)
                        continue;

                    double noteWidth = 30;
                    double noteHeight = note.Duration * pixelsPerSecond;

                    // плавное падение сверху: верхняя граница ноты стартует за экраном
                    double y = delta * pixelsPerSecond - noteHeight;

                    // ------------------------------------------------------
                    // 🎹 1. Проверяем: нота касается клавиатуры
                    // ------------------------------------------------------
                    double noteBottom = y + noteHeight;
                    double noteTop = y;

                    // верх клавиатуры = вся высота NotesCanvas
                    double keyboardTopY = NotesCanvas.RenderSize.Height;  // идимая высота элемента в текущем layout,
                                                                          // а не его растянутая высота в ScrollViewer

                    // включаем подсветку клавиши при касании
                    if (noteBottom >= keyboardTopY)
                    {
                        pianoVM.PressKey(note.NoteName);
                        note.HasPressed = true;
                    }

                    // снимаем нажатие, когда нота ушла полностью за нижний край Canvas
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

    }
}

