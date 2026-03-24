using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using VmesteApp.DB.Repository;
using VmesteApp.Security;

namespace VmesteApp
{
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
            LoadProfileData();
        }

        private void LoadProfileData()
        {
            if (App.CurrentUser != null)
            {
                UserNameTextBox.Text = App.CurrentUser.name;
                UserRoleTextBlock.Text = App.CurrentUser.role;
                UserEmailTextBox.Text = App.CurrentUser.email;
                UserPhoneTextBox.Text = App.CurrentUser.phone ?? "";
                FamilyIdTextBox.Text = App.CurrentUser.familyInviteCode;

                if (!string.IsNullOrEmpty(App.CurrentUser.avatarPath))
                {
                    try
                    {
                        string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Avatars", App.CurrentUser.avatarPath);
                        if (System.IO.File.Exists(fullPath))
                        {
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            AvatarImage.Source = bitmap;
                        }
                        else { SetDefaultAvatar(); }
                    }
                    catch { SetDefaultAvatar(); }
                }
            }
        }

        private void SetDefaultAvatar() => AvatarImage.Source = new BitmapImage(new Uri("pack://application:,,,/Avatars/nophoto.png"));

        // Продвинутая проверка домена (с защитой от зависания)
        public async Task<bool> IsDomainValid(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@")) return false;
            try
            {
                string domain = email.Split('@').Last();

                // Если нет интернета, пропускаем проверку (чтобы не выдавать ошибку "всегда")
                if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable()) return true;

                // Ограничиваем время ожидания ответа от DNS (2 секунды)
                var dnsTask = Dns.GetHostAddressesAsync(domain);
                if (await Task.WhenAny(dnsTask, Task.Delay(2000)) == dnsTask)
                {
                    var addresses = await dnsTask;
                    return addresses.Length > 0;
                }
                return true; // Если DNS не ответил быстро, не блокируем пользователя
            }
            catch { return false; }
        }

        // Обновленный асинхронный метод сохранения
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Блокируем кнопку, чтобы избежать двойных нажатий
            SaveButton.IsEnabled = false;

            try
            {
                string newName = UserNameTextBox.Text.Trim();
                string newEmail = UserEmailTextBox.Text.Trim().ToLower();
                string newPhone = UserPhoneTextBox.Text.Trim();
                string newPassword = NewPasswordBox.Password;

                // 1. Проверка на пустые поля
                if (string.IsNullOrWhiteSpace(newName))
                {
                    CustomMessageBox.Show("Имя не может быть пустым", "Ошибка");
                    return;
                }

                // 2. Строгий Regex (требует минимум 2 символа после последней точки)
                // Паттерн: ^[^@\s]+@[^@\s]+\.[^@\s]{2,}$
                string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$";

                if (!Regex.IsMatch(newEmail, emailPattern))
                {
                    CustomMessageBox.Show("Введите корректный Email (например, user@example.com)", "Ошибка валидации");
                    return;
                }

                // 3. Асинхронная проверка домена через DNS
                // Домен 'yande.r' не пройдет эту проверку, так как DNS не вернет IP
                if (!await IsDomainValid(newEmail))
                {
                    CustomMessageBox.Show("Указанный почтовый домен не существует", "Ошибка валидации");
                    return;
                }

                // 4. Валидация телефона
                string phonePattern = @"^\+7\s\(\d{3}\)\s\d{3}-\d{2}-\d{2}$";
                if (!Regex.IsMatch(newPhone, phonePattern))
                {
                    CustomMessageBox.Show("Введите телефон в формате: +7 (999) 000-00-00", "Ошибка валидации");
                    return;
                }

                var repo = new UserRepository();

                // Попытка обновления в БД
                repo.UpdateUserProfile(App.CurrentUser.userId, newName, newEmail, newPhone);

                // Обновляем данные в текущей сессии приложения
                App.CurrentUser.name = newName;
                App.CurrentUser.email = newEmail;
                App.CurrentUser.phone = newPhone;

                // Обновление пароля, если введено что-то новое
                if (!string.IsNullOrEmpty(newPassword) && newPassword != "newpassword")
                {
                    if (newPassword.Length < 6)
                    {
                        CustomMessageBox.Show("Пароль должен быть не короче 6 символов", "Внимание");
                        return;
                    }
                    repo.UpdatePassword(App.CurrentUser.userId, PasswordHasher.HashPassword(newPassword));
                    NewPasswordBox.Password = "newpassword";
                }

                CustomMessageBox.Show("Данные успешно обновлены!", "Успех");
                LoadProfileData();
            }
            catch (Exception ex)
            {
                // Сюда попадет ошибка про дубликат из UserRepository
                CustomMessageBox.Show(ex.Message, "Ошибка при сохранении");
            }
            finally
            {
                SaveButton.IsEnabled = true;
            }
        }

        // Обработчики маски телефона (без изменений)
        private void UserPhoneTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(UserPhoneTextBox.Text) || UserPhoneTextBox.Text == "+7 (999) 000-00-00")
            {
                UserPhoneTextBox.Text = "+7 (";
                UserPhoneTextBox.CaretIndex = UserPhoneTextBox.Text.Length;
            }
        }

        private void UserPhoneTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!UserPhoneTextBox.IsFocused) return;
            string digits = new string(UserPhoneTextBox.Text.Where(char.IsDigit).ToArray());
            string result = "+7 (";
            if (digits.Length <= 1) { UserPhoneTextBox.Text = result; UserPhoneTextBox.CaretIndex = result.Length; return; }
            string actualDigits = digits.Substring(1);
            if (actualDigits.Length > 0) result += actualDigits.Substring(0, Math.Min(actualDigits.Length, 3));
            if (actualDigits.Length > 3) result += ") " + actualDigits.Substring(3, Math.Min(actualDigits.Length - 3, 3));
            if (actualDigits.Length > 6) result += "-" + actualDigits.Substring(6, Math.Min(actualDigits.Length - 6, 2));
            if (actualDigits.Length > 8) result += "-" + actualDigits.Substring(8, Math.Min(actualDigits.Length - 8, 2));
            UserPhoneTextBox.Text = result;
            UserPhoneTextBox.CaretIndex = result.Length;
        }

        private void ChangeAvatarClick(object sender, RoutedEventArgs e) { /* ... ваш существующий код ... */ }
    }
}