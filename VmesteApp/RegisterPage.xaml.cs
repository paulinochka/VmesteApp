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
using VmesteApp.DB.Models;
using VmesteApp.DB.Repository;
using VmesteApp.Security;

namespace VmesteApp
{
    /// <summary>
    /// Логика взаимодействия для RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }

        private void RoleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 1. Самая важная проверка: если UI еще не отрисован, ничего не делаем
            if (!IsLoaded || FamilyCodeGroup == null)
                return;

            // 2. Получаем выбранный элемент через e.AddedItems (это надежнее)
            if (RoleComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string content = selectedItem.Content.ToString();

                if (content == "Участник семьи")
                {
                    FamilyCodeGroup.Visibility = Visibility.Visible;
                }
                else
                {
                    FamilyCodeGroup.Visibility = Visibility.Collapsed;
                    FamilyCodeBox.Text = string.Empty;
                }
            }
        }

        private void ReturnButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new LoginPage());
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if(!IsFormValid())
    {
                return;
            }
            var newUser = new Users
                {
                    name = NameBox.Text,
                    email = EmailBox.Text,
                    phone = PhoneBox.Text,
                    password = PasswordHasher.HashPassword(PassBox.Password),
                    role = RoleComboBox.Text,
                    familyId = 1
                };

                UserRepository repo = new UserRepository();
                if (repo.RegisterUser(newUser))
                {
                    CustomMessageBox.Show("Регистрация прошла успешно!", "Успех");
                    NavigationService.Navigate(new LoginPage());
                }
                else
                {
                    CustomMessageBox.Show("Ошибка при сохранении.", "Неудача");
                }
        }
        private bool IsFormValid()
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                CustomMessageBox.Show("Введите ваше полное имя", "Ошибка заполнения");
                return false;
            }

            if (string.IsNullOrWhiteSpace(EmailBox.Text) || !EmailBox.Text.Contains("@"))
            {
                CustomMessageBox.Show("Введите корректный Email", "Ошибка заполнения");
                return false;
            }

            if (string.IsNullOrWhiteSpace(PhoneBox.Text))
            {
                CustomMessageBox.Show("Введите номер телефона", "Ошибка заполнения");
                return false;
            }

            if (string.IsNullOrWhiteSpace(PassBox.Password))
            {
                CustomMessageBox.Show("Придумайте пароль", "Ошибка заполнения");
                return false;
            }

            if (RoleComboBox.SelectedItem == null)
            {
                CustomMessageBox.Show("Выберите вашу роль в семье", "Ошибка заполнения");
                return false;
            }

            return true;
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
