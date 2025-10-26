using PianoTrainerApp.Services;
using PianoTrainerApp.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace PianoTrainerApp.Views
{
    /// <summary>
    /// Логика взаимодействия для PianoWindow.xaml
    /// </summary>
    public partial class PianoWindow : Window
    {
        private PianoViewModel pianoVM;

        public PianoWindow(Song song)
        {
            InitializeComponent();
            pianoVM = new PianoViewModel();
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

            foreach (var note in pianoVM.FallingNotes)
            {
                double noteWidth = 30;
                double noteHeight = 20;

                // Прямоугольник
                var rect = new System.Windows.Shapes.Rectangle
                {
                    Width = noteWidth,
                    Height = noteHeight,
                    Fill = Brushes.DeepSkyBlue,
                    RadiusX = 6,
                    RadiusY = 6,
                    Stroke = Brushes.Black,
                    StrokeThickness = 0.8
                };

                // Подпись по центру
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

                // Центрируем по X и Y
                Canvas.SetLeft(rect, note.X);
                Canvas.SetTop(rect, note.Y);

                Canvas.SetLeft(text, note.X);
                Canvas.SetTop(text, note.Y + (noteHeight - text.FontSize) / 4);

                NotesCanvas.Children.Add(rect);
                NotesCanvas.Children.Add(text);
            }
        }
    }
}
