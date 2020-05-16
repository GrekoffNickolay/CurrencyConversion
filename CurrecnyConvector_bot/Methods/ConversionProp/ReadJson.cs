

namespace CurrecnyConvector_bot.Methods.ConversionProp
{
    /// <summary>
    /// Десериализация файля для конвертации
    /// </summary>
    public class ReadJson
    {
        public bool Success { get; set; }
        public Query query { get; set; }
        public Info info { get; set; }
        public string date { get; set; }
        public double result { get; set; }
        public class Info
        {
            public int timestamp { get; set; }
            public double rate { get; set; }
        }
        public class Query
        { 
            public string from { get; set; }
            public string to { get; set; }
            public int amount { get; set; }
        }
    }
}
