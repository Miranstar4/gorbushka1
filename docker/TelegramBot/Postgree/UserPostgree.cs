namespace TelegramBot.Postgree
{
    public class UserPostgree
    {
        public int id { get; set; }
        public long? telegramid { get; set; }
        public string? username { get; set; }
        public string? login { get; set; }
        public string? token { get; set; }
        public decimal? score { get; set; }
        public string? belt { get; set; }
        public decimal? woods { get; set; }
    }
}
