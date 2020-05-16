
namespace CurrecnyConvector_bot.Methods.LateastProp
{
    /// <summary>
    /// Класс для дессериализаиции файлаы
    /// </summary>
    public class ReadJson
    {
        public bool Success { get; set; }
        public int TimesTamp { get; set; }
        public string Base { get; set; }
        public string Date { get; set; }
        public Rate Rates { get; set; }
        public class Rate
        {
            public double RUB { get; set; }
        }
    }
}
