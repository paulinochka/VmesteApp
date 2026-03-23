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
using System.Windows.Navigation;
using System.Windows.Shapes;
using VmesteApp.DB.Models;
using VmesteApp.DB.Repository;

namespace VmesteApp
{
    /// <summary>
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string identifier = LoginBox.Text.Trim();
            string password = PassBox.Password;

            if (string.IsNullOrEmpty(identifier) || string.IsNullOrEmpty(password))
            {
                CustomMessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка");
                return;
            }

            UserRepository repo = new UserRepository();
            Users authenticatedUser = repo.Login(identifier, password);

            if (authenticatedUser != null)
            {
                // Сохраняем данные пользователя (например, в статический класс или свойство окна)
                // чтобы приложение знало, кто вошел
                App.CurrentUser = authenticatedUser;

                CustomMessageBox.Show($"Добро пожаловать, {authenticatedUser.name}!", "Успех");

                NavigationService.Navigate(new MainPage());
            }
            else
            {
                CustomMessageBox.Show("Неверная почта/телефон или пароль.", "Ошибка");
            }
        }

        private void RegisterButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new RegisterPage());
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.WindowState = WindowState.Minimized;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
