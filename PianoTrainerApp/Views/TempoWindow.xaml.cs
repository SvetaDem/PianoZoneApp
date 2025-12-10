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

namespace PianoTrainerApp.Views
{
    /// <summary>
    /// Логика взаимодействия для TempoWindow.xaml
    /// </summary>
    public partial class TempoWindow : Window
    {
        public double SelectedMultiplier { get; private set; } = 1.0;
        public TempoWindow()
        {
            InitializeComponent();
        }
        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            // Допустим, есть RadioButton или Slider с именем TempoSlider
            SelectedMultiplier = TempoSlider.Value; // например от 0.5 до 3.0
            DialogResult = true;
            Close();
        }
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
