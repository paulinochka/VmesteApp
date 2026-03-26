using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VmesteApp
{
    public class RegistrationValidator
    {
        public async Task<(bool IsValid, string Error)> ValidateAsync(
            string name, string email, string phone, string password, string role, string familyCode)
        {
            if (string.IsNullOrWhiteSpace(name))
                return (false, "Введите ваше полное имя");

            string emailPattern = @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\|\}_\w])*)(?<=[0-9a-z0-9])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$";

            if (string.IsNullOrWhiteSpace(email) || !Regex.IsMatch(email, emailPattern, RegexOptions.IgnoreCase))
                return (false, "Некорректный формат почты");

            string phonePattern = @"^\+7\s\(\d{3}\)\s\d{3}-\d{2}-\d{2}$";
            if (string.IsNullOrWhiteSpace(phone) || !Regex.IsMatch(phone, phonePattern))
                return (false, "Введите номер телефона в формате: +7 (999) 000-00-00");

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                return (false, "Пароль должен содержать минимум 6 символов");

            if (string.IsNullOrWhiteSpace(role))
                return (false, "Выберите вашу роль в семье");

            if (role == "Участник семьи" && string.IsNullOrWhiteSpace(familyCode))
                return (false, "Введите код семьи");

            return (true, null);
        }
    }
}
