using PianoTrainerApp.Models;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PianoTrainerApp.Views
{
    /// <summary>
    /// Логика взаимодействия для HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        // Событие для родителя
        public event Action<PageType> NavigateRequested;

        public HomeView()
        {
            InitializeComponent();
        }

        private void ButtonLibrary_Click(object sender, RoutedEventArgs e)
        {
            // Сообщаем родителю: открыть Library
            NavigateRequested?.Invoke(PageType.Library);
        }

        private void ButtonBeginner_Click(object sender, RoutedEventArgs e)
        {
            // Сообщаем родителю: открыть Beginner
            NavigateRequested?.Invoke(PageType.Beginner);
        }
    }
}
