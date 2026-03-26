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
using VmesteApp.DB.Repository;

namespace VmesteApp
{
    /// <summary>
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        private EventRepository _eventRepository = new EventRepository();
        public HomePage()
        {
            InitializeComponent();
            LoadData();
        }
        private void LoadData()
        {
            try
            {
                if (App.CurrentUser == null) return;

                int currentUserId = App.CurrentUser.userId;

                // Безопасное извлечение familyId (если null, то 0)
                int familyId = App.CurrentUser.familyId ?? 0;

                string userName = App.CurrentUser.name ?? "Пользователь";

                WelcomeText.Text = $"Добрый день, {userName}!";

                if (familyId != 0)
                {
                    TasksList.ItemsSource = _eventRepository.GetTasksForToday(familyId, currentUserId);
                    EventsList.ItemsSource = _eventRepository.GetUpcomingEvents(familyId, currentUserId);
                }
        }
            catch (Exception ex)
            {
                // Теперь CustomMessageBox покажет более детальную ошибку, если она останется
                CustomMessageBox.Show(ex.Message, "Ошибка загрузки данных");
            }
}
        private void ToCalendarPage_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CalendarPage());
        }
    }
}
