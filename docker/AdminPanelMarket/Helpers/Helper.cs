using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AdminPanelMarket.Helpers
{
    public static class Helper
    {
        public async static Task<List<string>> GetFilesFrom()
        {
            //String baseUrl = "https://localhost:7201";
            String baseUrl = "https://backend.hozyainduhi.ru";
            using (var client = new HttpClient())
            {

                var response = await client.GetAsync(baseUrl + "/api/ProductImages/GetFilesFrom");

                var responseContent = await response.Content.ReadAsStringAsync();

                if (!string.IsNullOrEmpty(responseContent))
                {
                    var result = JsonConvert.DeserializeObject<List<string>>(responseContent);
                    return result;
                }
            }
            return null;
        }
    }
}
