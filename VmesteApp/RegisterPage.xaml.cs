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
            // Имя
            if (string.IsNullOrWhiteSpace(NameBox.Text))
                return (false, "Введите ваше полное имя");

            // Почта: Продвинутый Regex
            string email = EmailBox.Text.Trim();
            string emailPattern = @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\|\}~w])*)(?<=[0-9a-z])@))" +
                                  @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$";

            if (string.IsNullOrWhiteSpace(email) || !Regex.IsMatch(email, emailPattern, RegexOptions.IgnoreCase))
                return (false, "Некорректный формат почты");

            // Почта: Проверка существования домена
            if (!await IsDomainValid(email))
                return (false, "Указанный почтовый домен не существует или недоступен");

            // Телефон
            string phonePattern = @"^\+7\s\(\d{3}\)\s\d{3}-\d{2}-\d{2}$";
            if (string.IsNullOrWhiteSpace(PhoneBox.Text) || !Regex.IsMatch(PhoneBox.Text, phonePattern))
                return (false, "Введите номер телефона в формате: +7 (999) 000-00-00");

            // Пароль
            if (string.IsNullOrWhiteSpace(PassBox.Password) || PassBox.Password.Length < 6)
                return (false, "Пароль должен содержать минимум 6 символов");

            // Роль и код семьи
            if (RoleComboBox.SelectedItem == null)
                return (false, "Выберите вашу роль в семье");

            string role = (RoleComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (role == "Участник семьи" && string.IsNullOrWhiteSpace(FamilyCodeBox.Text))
                return (false, "Введите код семьи");

            return (true, null);
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

        // --- Обработчики интерфейса (Маски и кнопки) ---

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