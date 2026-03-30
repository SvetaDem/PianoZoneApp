using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Логика взаимодействия для LessonsView.xaml
    /// </summary>
    public partial class LessonsView : UserControl
    {
        public ObservableCollection<Lesson> CurrentLessons { get; set; } = new ObservableCollection<Lesson>();

        private Dictionary<string, List<Lesson>> LessonsByCategory = new Dictionary<string, List<Lesson>>();

        public LessonsView()
        {
            InitializeComponent();

            // Пример видео (YouTube embed ссылки)
            LessonsByCategory["Новичок"] = new List<Lesson>
            {
                new Lesson { Title="Урок 1: Основы", VideoUrl="https://video.andreyshuvalov.ru/storage-360/Y1BoXB79sfY.mp4" },
                new Lesson { Title="Урок 2: Аккорды", VideoUrl="https://www.youtube.com/watch?v=osVxHTvmif4&list=PLmDM2kHkfG9WHFibz9x_TM9FxvlsJEYb9&index=2" }
            };
            LessonsByCategory["Средний"] = new List<Lesson>
            {
                new Lesson { Title="Урок 1: Арпеджио", VideoUrl="https://www.youtube.com/embed/VIDEO_ID3" },
                new Lesson { Title="Урок 2: Ладовые упражнения", VideoUrl="https://www.youtube.com/embed/VIDEO_ID4" }
            };
            LessonsByCategory["Продвинутый"] = new List<Lesson>
            {
                new Lesson { Title="Урок 1: Импровизация", VideoUrl="https://www.youtube.com/embed/VIDEO_ID5" },
                new Lesson { Title="Урок 2: Джазовые ходы", VideoUrl="https://www.youtube.com/embed/VIDEO_ID6" }
            };

            // Стартовая категория
            LoadCategory("Новичок");

            // Привязка
            LessonsListView.DataContext = this;
        }

        private void LoadCategory(string category)
        {
            CurrentLessons.Clear();
            foreach (var lesson in LessonsByCategory[category])
                CurrentLessons.Add(lesson);
        }

        private void CategoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                LoadCategory(btn.Content.ToString());
            }
        }

        // Загружаем видео в WebBrowser
        private void WebBrowser_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is WebBrowser wb && wb.DataContext is Lesson lesson)
            {
                wb.Navigate(lesson.VideoUrl);
            }
        }
    }

    public class Lesson
    {
        public string Title { get; set; }
        public string VideoUrl { get; set; }
    }
}
