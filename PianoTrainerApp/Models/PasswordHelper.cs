using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PianoTrainerApp.Models
{
    /// <summary>
    /// Вспомогательный класс для работы с паролями.
    /// Предоставляет методы для безопасной обработки паролей, например, для хэширования.
    /// </summary>
    public static class PasswordHelper
    {
        /// <summary>
        /// Создает SHA256-хэш для заданного пароля.
        /// </summary>
        /// <param name="password">Пароль в виде строки, который нужно захэшировать.</param>
        /// <returns>Возвращает строку в формате Base64, представляющую хэш пароля.</returns>
        /// <remarks>
        /// Метод использует алгоритм SHA256 для вычисления хэша.
        /// Хэширование позволяет безопасно хранить пароли без сохранения их в открытом виде.
        /// Base64 используется для удобного представления байтового массива в виде строки.
        /// </remarks>
        public static string HashPassword(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}
