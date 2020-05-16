using Newtonsoft.Json;
using System.Net;


namespace CurrecnyConvector_bot.Methods.ConversionProp
{
    /// <summary>
    /// Класс для конвертиации валют
    /// </summary>
    public class Converts
    {
        /// <summary>
        /// Api-key
        /// </summary>
        private const string accessKey = "0f34f937666f2020e379d4e2e8f19738";

        /// <summary>
        /// Класс для подготовки строки и отправкии ее
        /// </summary>
        /// <param name="from"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public string Conversions(string from, double amount)
        {
            // инициализайия переменной
            string currency;
            // инициализайия переменной
            string amountInStr;
            // перевод строки в строковой тип
            amountInStr = DoubleToString(amount);
            // присвоение переменной
            currency = ReadJson(from, amountInStr);
            //currency = ParseString(currency);
            currency = $"From {from} to RUB\n{currency}";

            // возвращение переменной
            return currency;
        }

        // Десериализация json  фала
        private string ReadJson(string fromCur, string amount, string toCur = "RUB")
        {
            // инициализайия переменной
            string historyCurrencies;

            // Чтение json  фала
            var jsonString = new WebClient().DownloadString("http://data.fixer.io/api/" + "convert" + "?access_key=" + accessKey + "&from=" + fromCur + "&to=" + toCur + "&amount=" + amount);

            // десериализация
            var jString = JsonConvert.DeserializeObject<ReadJson>(jsonString);
            
            // получение необходимого элемента
            historyCurrencies = jString.result.ToString();

            // возвращение переменной
            return historyCurrencies;
        }

        /// <summary>
        /// перевод double в string
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        private string DoubleToString(double d)
        {
            //перевод в string и замена запятых на точки
            string dStr = d.ToString().Replace(",",".");
            // // возвращение строки
            return dStr;
        }
    }
}
