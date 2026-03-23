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

namespace VmesteApp
{
    /// <summary>
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            InnerFrame.Navigate(new HomePage());
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new LoginPage());
        }

        // Метод для перехода на Главную
        private void HomePageButton_Click(object sender, RoutedEventArgs e)
        {
            // Замени HomePage на название твоей страницы главной
            InnerFrame.Navigate(new HomePage());
        }

        // Метод для перехода в Календарь
        private void CalendarButton_Click(object sender, RoutedEventArgs e)
        {
            InnerFrame.Navigate(new CalendarPage());
        }

        // Метод для перехода в Личный кабинет
        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            InnerFrame.Navigate(new ProfilePage());
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
