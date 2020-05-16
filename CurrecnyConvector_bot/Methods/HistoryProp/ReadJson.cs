using System.Collections.Generic;

namespace CurrecnyConvector_bot.Methods.HistoryProp
{
    /// <summary>
    /// Класс для десериализации файла
    /// </summary>
    class ReadJson
    {
        public MetaList meta { get; set; }
        public class MetaList
        {
            public Params effective_params { get; set; }
            public class Params
            {
                public string data_set { get; set; }
                public string[] base_currencies { get; set; }
                public string[] quote_currencies { get; set; }
                public string start_time { get; set; }
                public string end_time { get; set; }
                public string[] fields { get; set; }
            }
            public string endpoint { get; set; }
            public string request_time { get; set; }
            public string[] skipped_currency_pairs { get; set; }
        }
        public List<Quates> quotes { get; set; }
        public class Quates
        {
            public string base_currency { get; set; }
            public string quote_currency { get; set; }
            public string start_time { get; set; }
            public string open_time { get; set; }
            public string close_time { get; set; }
            public string average_bid { get; set; }
            public string average_ask { get; set; }
            public string average_midpoint { get; set; }

        }
    }
}