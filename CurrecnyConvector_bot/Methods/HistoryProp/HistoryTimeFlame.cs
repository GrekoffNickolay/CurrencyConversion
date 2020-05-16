using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace CurrecnyConvector_bot.Methods.HistoryProp
{
    /// <summary>
    /// Класс для прсмотра динамики калебаний курса
    /// </summary>
    class HistoryTimeFlame
    {
        /// <summary>
        /// Десериализация о отправка подоговленной строки 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="cur"></param>
        /// <returns></returns>
        public List<(string, string)> ReadJson(string from, string to, string cur)
        {
            // парсинг строки
            from = from.Replace(" ", "-");
            to = to.Replace(" ", "-");
            // инициализация переенных
            string jsonString = "";
            List<string> historyCurrencies;

            // создание списка
            List<(string, string)> jList = new List<(string, string)>();
            // парсинг строк, первод в DateTime
            DateTime fromDate = DateTime.Parse(from);
            DateTime toDate = DateTime.Parse(to);
           
            string tmp;
            string parseDate;

            string parseDateTo = ParseDate(toDate);

            // чтение файла
            try
            {
                parseDate = ParseDate(fromDate);
                // получение строки из файла
                jsonString = new WebClient().DownloadString($"https://www1.oanda.com/rates/api/v2/rates/candles.json?api_key=EMNppcP7tjLVu0RabBSOClRB&start_time={parseDate}T16:00:00+00:00&end_time={parseDateTo}T16:00:00+00:00&base={cur}&quote=RUB&fields=averages");
                // десериализация
                historyCurrencies = ParseOutputString(jsonString);
                // получение неохожимых элементов
                for (int i = 0; i < historyCurrencies.Count; i++)
                {
                    tmp = fromDate.ToShortDateString();
                    // запсь в спиок
                    jList.Add((tmp, historyCurrencies[i]));
                    fromDate = fromDate.AddDays(1);
                }
            }
            catch (Exception)
            {
                // обработка исклюений
                return null;
            }
            // отправка списка
            return jList;
        }
        /// <summary>
        /// парсинг даты
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private string ParseDate(DateTime date)
        {
            // инициализация переменных
            string parseDate = "";
            // парсинг строк
            string tmp = date.ToShortDateString().Replace(".", " ");
            // перевод в массив
            string[] tmps = tmp.Split(' ');
            parseDate += tmps[2] + "-";
            parseDate += tmps[1] + "-";
            parseDate += tmps[0];
            // отправка строки
            return parseDate;
        }

        /// <summary>
        /// Дисириализация файла
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        private List<string> ParseOutputString(string jsonString)
        {
            // получение десериализованного обекта
            ReadJson jString = JsonConvert.DeserializeObject<ReadJson>(jsonString);
            // создание списка
            List<string> historyCurrencies = new List<string>();
            //получение необходимого элемента
            for (int i = 0; i < jString.quotes.Count; i++)
            {
                // запис в список
                historyCurrencies.Add(jString.quotes[i].average_midpoint.ToString());
            }
            // отправка списка
            return historyCurrencies;
        }
    }
}
