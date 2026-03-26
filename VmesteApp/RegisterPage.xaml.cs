using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using VmesteApp.DB.Models;
using VmesteApp.DB.Repository;
using VmesteApp.Security;

namespace VmesteApp
{
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }

        // 1. Изменяем на async void, чтобы использовать await внутри
        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Блокируем кнопку, чтобы избежать повторных нажатий во время проверки DNS
            RegisterButton.IsEnabled = false;

            try
            {
                // Вызываем новую асинхронную валидацию
                var validationResult = await IsFormValidAsync();
                if (!validationResult.IsValid)
                {
                    CustomMessageBox.Show(validationResult.Error, "Ошибка заполнения");
                    return;
                }

                string selectedRole = (RoleComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? RoleComboBox.Text;

                var newUser = new Users
                {
                    name = NameBox.Text.Trim(),
                    email = EmailBox.Text.Trim().ToLower(), // Приводим к нижнему регистру
                    phone = PhoneBox.Text.Trim(),
                    password = PasswordHasher.HashPassword(PassBox.Password),
                    role = selectedRole
                };

                UserRepository repo = new UserRepository();

                if (repo.RegisterUser(newUser, FamilyCodeBox.Text.Trim()))
                {
                    CustomMessageBox.Show("Регистрация прошла успешно!", "Успех");
                    NavigationService.Navigate(new LoginPage());
                }
                else
                {
                    CustomMessageBox.Show("Не удалось сохранить данные пользователя.", "Неудача");
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(ex.Message, "Ошибка регистрации");
            }
            finally
            {
                RegisterButton.IsEnabled = true;
            }
        }

        // Продвинутая асинхронная проверка формы
        private async Task<(bool IsValid, string Error)> IsFormValidAsync()
        {
            var validator = new RegistrationValidator();

            string role = (RoleComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            return await validator.ValidateAsync(
                NameBox.Text.Trim(),
                EmailBox.Text.Trim(),
                PhoneBox.Text.Trim(),
                PassBox.Password,
                role,
                FamilyCodeBox.Text.Trim()
            );
        }

        public async Task<bool> IsDomainValid(string email)
        {
            try
            {
                string domain = email.Split('@').Last();
                // Пробуем получить IP-адреса домена
                var host = await Dns.GetHostEntryAsync(domain);
                return host.AddressList.Length > 0;
            }
            catch
            {
                return false;
            }
        }


        private void RoleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded || FamilyCodeGroup == null) return;

            if (RoleComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                FamilyCodeGroup.Visibility = selectedItem.Content.ToString() == "Участник семьи"
                    ? Visibility.Visible : Visibility.Collapsed;
                if (FamilyCodeGroup.Visibility == Visibility.Collapsed) FamilyCodeBox.Text = string.Empty;
            }
        }

        private void PhoneBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PhoneBox.Text))
            {
                PhoneBox.Text = "+7 (";
                PhoneBox.CaretIndex = PhoneBox.Text.Length;
            }
        }

        private void PhoneBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!PhoneBox.IsFocused) return;
            string digits = new string(PhoneBox.Text.Where(char.IsDigit).ToArray());
            string result = "+7 (";
            if (digits.Length <= 1) { PhoneBox.Text = result; PhoneBox.CaretIndex = result.Length; return; }

            string actualDigits = digits.Substring(1);
            if (actualDigits.Length > 0) result += actualDigits.Substring(0, Math.Min(actualDigits.Length, 3));
            if (actualDigits.Length > 3) result += ") " + actualDigits.Substring(3, Math.Min(actualDigits.Length - 3, 3));
            if (actualDigits.Length > 6) result += "-" + actualDigits.Substring(6, Math.Min(actualDigits.Length - 6, 2));
            if (actualDigits.Length > 8) result += "-" + actualDigits.Substring(8, Math.Min(actualDigits.Length - 8, 2));

            PhoneBox.Text = result;
            PhoneBox.CaretIndex = result.Length;
        }

        private void ReturnButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => NavigationService.Navigate(new LoginPage());
        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => Window.GetWindow(this).WindowState = WindowState.Minimized;
        private void CloseButton_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
    }
}