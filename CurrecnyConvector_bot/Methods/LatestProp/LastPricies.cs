using Newtonsoft.Json;
using System;
using System.Net;

namespace CurrecnyConvector_bot.Methods.LateastProp
{
    /// <summary>
    /// класс для показа актуальных курсов валют
    /// </summary>
    class LastPricies
    {
        /// <summary>
        /// Api-key
        /// </summary>
        private const string accessKey = "0f34f937666f2020e379d4e2e8f19738";

        /// <summary>
        /// Метода возвращает актуальные курсы валют
        /// </summary>
        /// <returns></returns>
        public string InfoAboautMainCurrencies()
        {
            // инициализаиця и присвоение занчений
            string currency;

            // from
            string usd = "USD",
                   eur = "EUR",
                   cny = "CNY",
                   gbp = "GBP";

            // to
            string rub = "RUB";

            // запись информациии о курсе валюты USD
            string usdcur = ReadJson(usd, rub);
            // запись информациии о курсе валюты EUR
            string eurcur = ReadJson(eur, rub);
            // запись информациии о курсе валюты CNY
            string cnycur = ReadJson(cny, rub);
            // запись информациии о курсе валюты GBP
            string gbrcur = ReadJson(gbp, rub);

            // создание подготовленной строки
            currency = $"Биржевой курс на {DateTime.Now.ToString("hh:mm")} мск. {DateTime.Now.ToString("yyyy-MM-dd")}\n" +
                        $"Доллар США:\t {usdcur} руб.\n" +
                        $"Евро:\t {eurcur} руб.\n" +
                        $"Фунт стерлингов:\t {gbrcur} руб.\n" +
                        $"Китайский юань:\t {cnycur} руб.";
           // отправка строки
            return currency;

        }
        /// <summary>
        /// десериализаиця файла
        /// </summary>
        /// <param name="fromCur"></param>
        /// <param name="toCur"></param>
        /// <returns></returns>
        private string ReadJson(string fromCur, string toCur = "RUB")
        {
            string historyCurrencies;

            var jsonString = new WebClient().DownloadString("http://data.fixer.io/api/" + "latest" + "?access_key=" + accessKey + "&base=" + fromCur + "&symbols=" + toCur + "&amount=1");
            //http://data.fixer.io/api/latest?access_key=0f34f937666f2020e379d4e2e8f19738&base=USD&symbols=RUB&amount=1
            var jString = JsonConvert.DeserializeObject<ReadJson>(jsonString);
            Console.WriteLine(jString.ToString());
            historyCurrencies = jString.Rates.RUB.ToString();

            return historyCurrencies;
        }
    }
}
