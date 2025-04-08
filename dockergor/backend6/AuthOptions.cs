using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace backend6
{
    public class AuthOptions
    {
        public const string ISSUER = "TelegramAPP"; // издатель токена
        public const string AUDIENCE = "TelegramAPPClient"; // потребитель токена
        const string KEY = "mysupersecret_secretkey!123";   // ключ для шифрации
        public const int LIFETIME = 640000000; // время жизни токена - 1 минута
        public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
    }
}
