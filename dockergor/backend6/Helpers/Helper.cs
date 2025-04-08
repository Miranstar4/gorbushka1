using System.Security.Cryptography;

namespace backend6.Helpers
{
    static public class Helper
    {
        static public string GenerateToken(int length)
        {
            byte[] randomBytes = new byte[length / 2];
            using (RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetBytes(randomBytes);
            }

            string token = BitConverter.ToString(randomBytes).Replace("-", "");

            return token;
        }
    }
}
