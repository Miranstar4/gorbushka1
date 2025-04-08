using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace backend6.Helpers
{
    static public class ValidateBotHash
    {
        static public bool Validate(string initData)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json").Build();

            var bottoken = config.GetSection("BotToken").Value;
            
            // Parse string initData from telegram.
            var data = HttpUtility.ParseQueryString(initData);

            // Put data in a alphabetically sorted dict.
            var dataDict = new SortedDictionary<string, string>(
                data.AllKeys.ToDictionary(x => x!, x => data[x]!),
                StringComparer.Ordinal);

            // Constant key to genrate secret key.
            var constantKey = "WebAppData";

            // https://core.telegram.org/bots/webapps#validating-data-received-via-the-web-app:
            // Data-check-string is a chain of all received fields,
            // sorted alphabetically.
            // in the format key=<value>.
            // with a line feed character ('\n', 0x0A) used as separator.
            // e.g., 'auth_date=<auth_date>\nquery_id=<query_id>\nuser=<user>'
            var dataCheckString = string.Join(
                '\n', dataDict.Where(x => x.Key != "hash") // Hash should be removed.
                    .Select(x => $"{x.Key}={x.Value}")); // like auth_date=<auth_date> ..

            // secrecKey is the HMAC-SHA-256 signature of the bot's token
            // with the constant string WebAppData used as a key.
            var secretKey = HMACSHA256.HashData(
                Encoding.UTF8.GetBytes(constantKey), // WebAppData
                Encoding.UTF8.GetBytes(bottoken)); // Bot's token

            var generatedHash = HMACSHA256.HashData(
                secretKey,
                Encoding.UTF8.GetBytes(dataCheckString)); // data_check_string

            // Convert received hash from telegram to a byte array.
            var actualHash = Convert.FromHexString(dataDict["hash"]); // .NET 5.0 

            // Compare our hash with the one from telegram.
            if (actualHash.SequenceEqual(generatedHash))
            {
                return true;
            }
            return false;
        }
    }
}
