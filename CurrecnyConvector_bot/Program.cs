using System;
using Telegram.Bot;
using System.Threading;
using System.Collections.Generic;
using Telegram.Bot.Args;
using System.Threading.Tasks;
using System.IO;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Drawing;
using System.Drawing.Imaging;
using CurrecnyConvector_bot.Methods.LateastProp;
using CurrecnyConvector_bot.Methods.ConversionProp;
using CurrecnyConvector_bot.Methods.HistoryProp;

namespace CurrecnyConvector_bot
{
    class Program
    {
        /// <summary>
        /// Телеграм бот клиент
        /// </summary>
        private static TelegramBotClient _botClient;
        /// <summary>
        /// Словарь для отслеживания состояния
        /// </summary>
        private static Dictionary<int, int> _stateProvider = new Dictionary<int, int>();
        // Токен бота
        private static string token = "";

        // Дата начала периода
        private static  string startDate;
        // Дата конца периода
        private static string endDate;

        // Название валюты
        private static string buttonVal;

        /// <summary>
        /// Точка входы в программа
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            try
            {
                // Инициализация телеграм клиента
                _botClient = new TelegramBotClient(token);

                // Присоеденение пользвотеля
                var me = _botClient.GetMeAsync().Result;

                // Дабавление в событе метод
                _botClient.OnMessage += BotOnMessageRecived;

                // Дабавление в событе метод
                _botClient.OnCallbackQuery += BotOnCallbackQueryRecived;

                // Старт обновления сообщений от пользвателя
                _botClient.StartReceiving();
                // Настраиваю интервал на 10 секунд
                Thread.Sleep(10);
            }
            catch (Exception ex)
            {
                // ОБработка исключения
                Console.WriteLine($"{new String('=', 30)}\nОШИБКА: {ex.Message}");
            }
            Console.ReadLine();
        }

        /// <summary>
        /// Метод для обработки Query запросов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static async void BotOnCallbackQueryRecived(object sender, CallbackQueryEventArgs e)
        {
            // Переменая, в которой хронится назвние валюты
            string buttonText;

            // Инициализация переменной
            buttonText = e.CallbackQuery.Data;

            //  Проверка на наличие состояния
            if (_stateProvider.ContainsKey(201))
            {
                // Удаление состояния
                _stateProvider.Remove(201);

                // Проверка на наличие состояния 
                if(_stateProvider.ContainsKey(205) && _stateProvider.ContainsValue(e.CallbackQuery.From.Id))
                    _stateProvider.Remove(205);// удаление состояния

                // Отправка пользователю сообщения
                await _botClient.SendTextMessageAsync(e.CallbackQuery.From.Id, $"Вы выбрали {buttonText}. Введите сумму конвертации");

                // Парсинг строки
                buttonVal = buttonText.Substring(0, 3);

                // Добавление состояния
                _stateProvider.Add(205, e.CallbackQuery.From.Id);
            }
            // Проверка на наличие состояния
            if (_stateProvider.ContainsKey(203))
            {
                // Удаление состяния
                _stateProvider.Remove(203);

                // Проверка на наличие состояния
                if (_stateProvider.ContainsKey(206))
                    _stateProvider.Remove(206); // удаление состояния

                // Отправка пользвоателю сообщения
                await _botClient.SendTextMessageAsync(e.CallbackQuery.From.Id, $"Вы выбрали {buttonText}. Введите дату начала поиска в формате (YYYY MM DD).");

                // Присваивание глобальной переменной значение
                buttonVal = buttonText.Substring(0, 3);
               
                // Удаление состояния
                _stateProvider.Add(206,  e.CallbackQuery.From.Id);
            }
        }

        // Метод для обработки текстовых сообщений
        private static async void BotOnMessageRecived(object sender, MessageEventArgs e)
        {
            // Инициализация переменной
            var message = e.Message;
            // Проверка: пустая ли строка?
            if (message == null || message.Type != MessageType.Text)
                return;

            // Инициализация переменной
            string name;
            // Инициализация переменной
            string log;

            // Присваивание строке значение
            name = $"{message.From.FirstName}{message.From.LastName}";

            // Присваивание строке значение
            log = $"{name} отправил  {message.Text};";

            Console.WriteLine(log);

            // Проверка на наличие состояния
            if (_stateProvider.ContainsKey(205))
            {
                // Инициализация переменной
                (double, string) amount;
                // Инициализация переменной
                string converts;

                // Присваивание картежу значение
                amount = ReadValOfAmount(message);
                // Если значение не равно нулю, 
                if (amount.Item2 != null)
                {
                    // то пользоватлю отправляется сообщение о не корректных входных параметрах
                    await _botClient.SendTextMessageAsync(message.Chat.Id, amount.Item2);
                }
                else 
                {
                    // иначе создается экземпляр класса
                    var conobj = new Converts();
                    // Вызывается метод и присваевается значение 
                    converts = conobj.Conversions(buttonVal, amount.Item1);
                    // Результат отправляется пользователю
                    await _botClient.SendTextMessageAsync(message.Chat.Id, converts);
                    // Удаляется состояние
                    _stateProvider.Remove(205);
                }
            }
            // Проверка наличия состояния
            if (_stateProvider.ContainsKey(207))
            {
                // инициализация переменной
                string infoFirstDate;
                // присваение переменной значения даты
                infoFirstDate = await ReadLastDate(message);
                // если значние не равно null 
                if (infoFirstDate != null)
                {
                    // то это значение присваевается  
                    endDate = infoFirstDate;
                    // создание объекта класса
                    var history = new HistoryTimeFlame();
                    // Инициализицаия списка картежей
                    List<(string, string)> timeflame = history.ReadJson(startDate, endDate, buttonVal);
                    // Инициализиация переменной
                    string output = "";
                    // Присваивание значений строке
                    for (int i = 0; i < timeflame.Count; i++)
                    {
                        output += $"Дата:  {timeflame[i].Item1} ,  Курс:   {timeflame[i].Item2}\n";

                    }
                    // Отправка сообщения пользвоателю
                    await _botClient.SendTextMessageAsync(message.From.Id, output);
                    // создание объекта класса  
                    GraphCreation graphObj = new GraphCreation();
                    // инициализаиця переменной и присвоение ей значение 
                    Bitmap graph = graphObj.DrawGraph(startDate, endDate, buttonVal, timeflame);
                    // Сохранение перемнной в буфере
                    using (MemoryStream ms = new MemoryStream())
                    {
                        // Сохранение перемнной в буфере в формате Png
                        graph.Save(ms, ImageFormat.Png);
                        ms.Position = 0;
                        // Отправка сообщения
                        await _botClient.SendPhotoAsync(message.From.Id, ms);
                    }
                    // Удаление состояния
                    _stateProvider.Remove(207);
                }
            }

            // Проверка наличия состояния
            if (_stateProvider.ContainsKey(206) )
            {
                // Инициализация переменной
                string infoFirstDate;
                // присвоение ей значения
                infoFirstDate = await ReadFirstDate(message);

                // если значние  равно null, то не заходим в метод
                if (!(infoFirstDate == null))
                {
                    // удаление состояния
                    _stateProvider.Remove(206);
                    // Присвоние ей значения
                    startDate = infoFirstDate;
                    // Отправка сообщения 
                    await LastStepHistory(message);
                    // удаление состояния
                    _stateProvider.Add(207, message.From.Id);
                }
            }
            // проверка сообщяния. имеет ли он следующии значения, если да то вызывается соответсвующий метод
            switch (message.Text)
            {
                case "/start":
                   await AllCommands(message);
                    break;
                case "/history":
                    await HistoryCurrencies(message);
                    break;
                case "/conversion":
                    await Conversion(message);
                    break;
                case "/latest":
                    await LatestCurrenciesPrices(message);
                    break;
                case "/info":
                    await InfoAboutBot(message);
                    break;
                case "Работа с валютами":
                    await CommandsOfCurrensies(message);
                    break;
                case "Информация по командам бота":
                    await InfoAboutBot(message);
                    break;
                case "Актуальные курсы валют":
                    await LatestCurrenciesPrices(message);
                    break;
                case "Перерасчет валют":
                    await Conversion(message);
                    break;
                case "Исторические курсы":
                    await HistoryCurrencies(message);
                    break;
                case "Назад":
                    if (_stateProvider.ContainsKey(207))
                        await CommandsOfCurrensies(message);
                    else if (_stateProvider.ContainsKey(202))
                        await Conversion(message);
                    else if (_stateProvider.ContainsKey(102))
                        await CommandsOfCurrensies(message);
                    else
                        await AllCommands(message);
                    break;
                case "Основной список":
                    await MainListToConversion(message);
                    break;
                case "Расширенный список":
                    await ExtendListToConversion(message);
                    break;
                case "Все":
                    await AllListToConversion(message);
                    break;
                case "Европа":
                    await Europe(message);
                    break;
                case "Средний восток":
                    await MiddleEast(message);
                    break;
                case "Антильские/Карибские острова":
                    await AntillesAndCaribeanIslands(message);
                    break;
                case "Южная Америка":
                    await SouthAmerica(message);
                    break;
                case "Северная Америка":
                    await NorthAmerica(message);
                    break;
                case "Африка":
                    await Africa(message);
                    break;
                case "Азия + остальное":
                    await Other(message);
                    break;
            }
        }
        /// <summary>
        /// Метода возвращает клавиатуру
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        static async Task AllCommands(Message message)
        {
            // Проверка наличия состояния
            if (_stateProvider.ContainsKey(100))
                _stateProvider.Remove(100);// если такое состояние имеется, то оно удаляется
            // Состоявление клавиатуры
            var keyboardStep = new ReplyKeyboardMarkup(new[]
            {
               new[]
               {
                    new KeyboardButton("Работа с валютами"),

               },
               new[]
               {
                    new KeyboardButton("Информация по командам бота"),
               }
            });
            // Добавленияе состояния
            _stateProvider.Add(100, message.From.Id);
            //отправка пользователю клавиатуры с сообщением
            await _botClient.SendTextMessageAsync(message.From.Id, "Главное меню", replyMarkup: keyboardStep);
        }

        /// <summary>
        /// Метод возвращает сообщение по командам бота
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        static async Task InfoAboutBot(Message message)
        {
            // инициализация строки и присвонение ей значнеия
            string text = "Что может данный бот?\n" +
                          "1) Может конвертровать валюты\n" +
                          "2) Показывать актуальные курсы валют\n" +
                          "3) Показывать динамику изменения курса\n";
            // Отправка сообщения
            await _botClient.SendTextMessageAsync(message.From.Id, text);
        }
        /// <summary>
        ///  Метод возвращает встроенную клавиатуру с операциями над валютами
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        static async Task CommandsOfCurrensies(Message message)
        {
            // проверка налиячия состояния
            if (_stateProvider.ContainsKey(101))
                _stateProvider.Remove(101);// удаление состояния
            // Состоявление клавиатуры
            var keyboardStep = new ReplyKeyboardMarkup(new[]
                {
                new[]
                {
                    new KeyboardButton("Актуальные курсы валют"),
                    new KeyboardButton("Перерасчет валют"),

                },
                new[]
                {
                    new KeyboardButton("Исторические курсы"),
                    new KeyboardButton("Назад")
                }
            });
            // Добавление состояния
            _stateProvider.Add(101, message.From.Id);
            // отправляется сообщение
            await _botClient.SendTextMessageAsync(message.From.Id, "Вы выбрали: Работа с валютами", replyMarkup: keyboardStep);

        }

        /// <summary>
        /// Метод возвращает актуальные курсы валют
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        static public async Task LatestCurrenciesPrices(Message message)
        {
            // создание обхекта
            LastPricies pricies = new LastPricies();
            // инициализация переменной
            string valOfCurrencies;
            // присвоение значения переменной 
            valOfCurrencies = pricies.InfoAboautMainCurrencies();
           // отправка пользователю сообщения
            await _botClient.SendTextMessageAsync(message.From.Id, valOfCurrencies);
        }
        /// <summary>
        /// Метода возвращает сумму конвертации из одной ваюты в другую
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        static async Task Conversion(Message message)
        {
            // проверка наличий состояний, в случае елси они имеется, то удаляется
            if (_stateProvider.ContainsKey(102))
                _stateProvider.Remove(102);
            if (_stateProvider.ContainsKey(101))
                _stateProvider.Remove(101);
            if (_stateProvider.ContainsKey(202))
                _stateProvider.Remove(202);
            // составление клавиатуры
            var keyboardStep = new ReplyKeyboardMarkup(new[]
            {
                new[]
                {
                    new KeyboardButton("Основной список"),
                    new KeyboardButton("Расширенный список"),

                },
                new[]
                {
                    new KeyboardButton("Все"),
                },
                new[]
                {
                    new KeyboardButton("Назад")
                }
            });
            // удаление состояния
            _stateProvider.Add(102, message.From.Id);
            // отправка пользоватлю клавиатуры + сообщение
            await _botClient.SendTextMessageAsync(message.From.Id, "Для более простого выбора нужной валюты, выберете необходимую категорию", replyMarkup: keyboardStep);

        }

        /// <summary>
        /// Метод возвращает Inline клавиатуру
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        static async Task HistoryCurrencies(Message message)
        {
            // проверка наличия состояния
            if (_stateProvider.ContainsKey(203))
                _stateProvider.Remove(203);// удаление состояния
            // составление клавиатуры
            var keyboardStep = new InlineKeyboardMarkup(new[]
               {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("USD 🇺🇸"),
                    InlineKeyboardButton.WithCallbackData("EUR 🇪🇺"),
                    InlineKeyboardButton.WithCallbackData("GBR 🇬🇧"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("CNY 🇨🇳"),
                    InlineKeyboardButton.WithCallbackData("INR 🇮🇳"),
                    InlineKeyboardButton.WithCallbackData("CHF 🇨🇭"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("AUD 🇦🇺"),
                    InlineKeyboardButton.WithCallbackData("BRL 🇧🇷"),
                    InlineKeyboardButton.WithCallbackData("HUF 🇭🇺"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("DKK 🇩🇰"),
                    InlineKeyboardButton.WithCallbackData("ILS 🇮🇱"),
                    InlineKeyboardButton.WithCallbackData("HKD 🇭🇰"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("IDR 🇮🇩"),
                    InlineKeyboardButton.WithCallbackData("CAD 🇨🇦"),
                    InlineKeyboardButton.WithCallbackData("KRW 🇰🇷"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("MYR 🇲🇾"),
                    InlineKeyboardButton.WithCallbackData("MXN 🇮🇹"),
                    InlineKeyboardButton.WithCallbackData("NZD 🇳🇿"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("NOK 🇳🇴"),
                    InlineKeyboardButton.WithCallbackData("PKR 🇵🇰"),
                    InlineKeyboardButton.WithCallbackData("PLN 🇵🇱"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("SGD 🇸🇬"),
                    InlineKeyboardButton.WithCallbackData("ISK 🇹🇷"),
                    InlineKeyboardButton.WithCallbackData("THB 🇨🇷"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("TRY 🇹🇷"),
                    InlineKeyboardButton.WithCallbackData("PHP 🇸🇽"),
                    InlineKeyboardButton.WithCallbackData("CZK 🇨🇿"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("SEK 🇸🇪"),
                    InlineKeyboardButton.WithCallbackData("ZAR 🇿🇦 "),
                    InlineKeyboardButton.WithCallbackData("INR 🇮🇳"),
                },
            });
            // Добавление клавиатуры
            _stateProvider.Add(203, message.From.Id);
            // Отправка пользвоателю клавиатуры
            await _botClient.SendTextMessageAsync(message.From.Id, "Выберете валюту.", replyMarkup: keyboardStep);

        }

        /// <summary>
        /// Метод считывает дату и проверяет ее на корректность
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        static async Task<string> ReadFirstDate(Message message)
        {
            // иницаилизация переменной и присвоение ей значения
            (string, bool, string) infoAboutOfDate = ReadDate(message);

            // проверка корректной даты
            if (!infoAboutOfDate.Item2)
            {
                // отправка пользователю сообщения о некорректном вводе
                await _botClient.SendTextMessageAsync(message.From.Id, $"{infoAboutOfDate.Item3}\nПовторите запрос, указав дату в данном формате - YYYY MM DD");
                return null;
            }
            else
            {
                // взвращение даты
                return infoAboutOfDate.Item1;
            }
        }

        /// <summary>
        /// метод проверяет первую дату
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        static (string, bool, string) ReadDate(Message message)
        {
            // разбиывается строка на массиа 
            string[] parseDate = message.Text.Split(' ');
            // если в строке не три элемента, то возвращается строка
            if (parseDate.Length != 3)
                return (null, false, "Неверный формат даты!");
            // иницаилизация переменных
            int year;
            int month;
            int day;
            // проверка строки на на корректный ввод
            if (int.TryParse(parseDate[0], out year)
                && int.TryParse(parseDate[1], out month)
                && int.TryParse(parseDate[2], out day))
            {
                if ((year > DateTime.Now.Year || year < 2000
                || month > 12 || month < 1
                || day > 31 || day < 1))
                    return (null, false, "Некорректный ввод, неверно указана дата!\nМожно вводить дату не раньше - 2000.01.01");
                string date;
                date = year.ToString();
                date += " " + month.ToString();
                date += " " + day.ToString();
                return (date, true, null);
            }
            return (null, false, "Некорректный ввод, имеются посторонние символы!");
        }

        /// <summary>
        /// метод считывает вторую дату
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        static async Task<string> ReadLastDate(Message message)
        {
            // если пользователь ввел "назад", то выполняется следующий код
            if (message.Text == "Назад")
            {
                // вызов метода
                await CommandsOfCurrensies(message);
                return null;
            }
            else
            {
                // чтение даты
                (string, bool, string) infoAboutOfDate = ReadDate(message);
                // если значение равно false, то 
                if (!infoAboutOfDate.Item2)
                {
                    // отправляется сообщение о некорректном вводе данных
                    await _botClient.SendTextMessageAsync(message.From.Id, $"{infoAboutOfDate.Item3}\nПовторите запрос, указав дату в данном формате - YYYY MM DD");
                    return null;
                }
                else
                {
                   //парсинг строк
                    startDate.Replace(" ", ".");
                    infoAboutOfDate.Item1.Replace(" ", ".");

                    //парсинг в DateTime формат
                    DateTime from = DateTime.Parse(startDate);
                    DateTime to = DateTime.Parse(infoAboutOfDate.Item1);
                    
                    // дальнейшая проверка корректности даты
                    if (from >= to)
                    {
                        Console.WriteLine(1);
                        await _botClient.SendTextMessageAsync(message.From.Id, $"Последняя дата не может быть больше первой");
                        return null;
                    }
                    if ((to.Year - from.Year) > 1)
                    {
                        Console.WriteLine(2);
                        await _botClient.SendTextMessageAsync(message.From.Id, $"Переод не может быть больше месяца");
                        return null;
                    }
                    if ((to.Month - from.Month) > 1)
                    {
                        Console.WriteLine(3);
                        await _botClient.SendTextMessageAsync(message.From.Id, $"Переод не может быть больше месяца");
                        return null;
                    }
                    // возвращается дата
                    return infoAboutOfDate.Item1;
                }
            }

        }

        /// <summary>
        /// Метод отправляет клавиатуру
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        static async Task LastStepHistory(Message message)
        {
            // состоявлние клавиатуры
            var keyboardStep = new ReplyKeyboardMarkup(new[]
                      {
                        new[]
                        {
                            new KeyboardButton("Назад"),
                         }
                    });
            // отправка пользователю клавиатуры
            await _botClient.SendTextMessageAsync(message.From.Id, "Введите дату конца поиска в формате (YYYY MM DD).", replyMarkup: keyboardStep);

        }
        /// <summary>
        /// метод возвращает клавиатуру
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        static async Task MainListToConversion(Message message)
        {
            // проверка наличия состяния
            if (_stateProvider.ContainsKey(201))
                _stateProvider.Remove(201);
            // состоявлние клавиатуры
            var keyboardStep = new InlineKeyboardMarkup(new[]
              {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("USD 🇺🇸"),
                    InlineKeyboardButton.WithCallbackData("EUR 🇪🇺"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("GBR 🇬🇧"),
                    InlineKeyboardButton.WithCallbackData("CHF 🇨🇭"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("CNY 🇨🇳"),
                    InlineKeyboardButton.WithCallbackData("JPY 🇯🇵"),

                },
            });
            // добавлние состяния
            _stateProvider.Add(201,message.From.Id);
            // отправка пользоваетлю клаватуры
            await _botClient.SendTextMessageAsync(message.From.Id, "Основные валюты", replyMarkup: keyboardStep);

        }

        /// <summary>
        /// метод отправляет пользвотелю клавиатуру
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        static async Task ExtendListToConversion(Message message)
        {
            if (_stateProvider.ContainsKey(201))
                _stateProvider.Remove(201);
            var keyboardStep = new InlineKeyboardMarkup(new[]
                 {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("USD 🇺🇸"),
                    InlineKeyboardButton.WithCallbackData("EUR 🇪🇺"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("GBR 🇬🇧"),
                    InlineKeyboardButton.WithCallbackData("CHF 🇨🇭"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("CNY 🇨🇳"),
                    InlineKeyboardButton.WithCallbackData("INR 🇮🇳"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("AUD 🇦🇺"),
                    InlineKeyboardButton.WithCallbackData("BRL 🇧🇷"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("HUF 🇭🇺"),
                    InlineKeyboardButton.WithCallbackData("HKD 🇭🇰"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("DDK 🇩🇰"),
                    InlineKeyboardButton.WithCallbackData("ILS 🇮🇱"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("IDR 🇮🇩"),
                    InlineKeyboardButton.WithCallbackData("CAD 🇨🇦"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("KRW 🇰🇷"),
                    InlineKeyboardButton.WithCallbackData("MYR 🇲🇾"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("MXN 🇮🇹"),
                    InlineKeyboardButton.WithCallbackData("NZD 🇳🇿"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("NOK 🇳🇴"),
                    InlineKeyboardButton.WithCallbackData("PKR 🇵🇰"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("PLN 🇵🇱"),
                    InlineKeyboardButton.WithCallbackData("SGD 🇸🇬"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("TWD 🇹🇼"),
                    InlineKeyboardButton.WithCallbackData("THB 🇨🇷"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("TRY 🇹🇷"),
                    InlineKeyboardButton.WithCallbackData("PHP 🇸🇽"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("CZK 🇨🇿"),
                    InlineKeyboardButton.WithCallbackData("CLP 🇨🇱"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("SEK 🇸🇪"),
                    InlineKeyboardButton.WithCallbackData("SAR 🇸🇦"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("ZAR 🇿🇦 "),
                    InlineKeyboardButton.WithCallbackData("INR 🇮🇳"),

                },
            });
            _stateProvider.Add(201, message.From.Id);
            await _botClient.SendTextMessageAsync(message.From.Id, "Расширенный список валют", replyMarkup: keyboardStep);

        }
        /// <summary>
        /// Метод отправляет пользователю клавиатуру с названием кантиненов
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        static async Task AllListToConversion(Message message)
        {
            if (_stateProvider.ContainsKey(202) )
                _stateProvider.Remove(202);
            var keyboardStep = new ReplyKeyboardMarkup(new[]
               {
                new[]
                {
                    new KeyboardButton("Южная Америка" /*🇦🇹 🇦🇩 🇧🇪 🇩🇪 🇬🇷 🇮🇪 🇪🇸"*/),
                    new KeyboardButton("Северная Америка" /*🇧🇭 🇧🇳 🇪🇬 🇮🇱 🇯🇴"*/),
                },
                new[]
                {
                    new KeyboardButton("Антильские/Карибские острова"/* 🇫🇰 🇦🇼 🇧🇸 🇧🇲ь 🇧🇧"*/),
                },
                new[]
                {
                    new KeyboardButton("Европа"/* 🇦🇷 🇧🇴 🇧🇷 🇨🇴"*/),
                    new KeyboardButton("Средний восток"/* 🇬🇱 🇱🇷 🇨🇦 🇺🇸"*/),
                    new KeyboardButton("Африка"/* 🇦🇱 🇧🇪 🇩🇯"*/),
                },
                new[]
                {
                    new KeyboardButton("Азия + остальное"),
                    new KeyboardButton("Назад")
                }
            });
            _stateProvider.Add(202, message.From.Id);
            await _botClient.SendTextMessageAsync(message.From.Id, "Выберете континент", replyMarkup: keyboardStep);

        }
        /// <summary>
        /// Метод отправляет пользователю клавиатуру
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        static async Task SouthAmerica(Message message)
        {
            if (_stateProvider.ContainsKey(201))
                _stateProvider.Remove(201);

            var keyboardStep = new InlineKeyboardMarkup(new[]
               {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("ARS"),
                    InlineKeyboardButton.WithCallbackData("BOB"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("BOB"),
                    InlineKeyboardButton.WithCallbackData("BRL"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("VES"),
                    InlineKeyboardButton.WithCallbackData("GYD"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("GTQ"),
                    InlineKeyboardButton.WithCallbackData("HNL"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("COP"),
                    InlineKeyboardButton.WithCallbackData("CRC"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("MXN"),
                    InlineKeyboardButton.WithCallbackData("NIO"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("PYG"),
                    InlineKeyboardButton.WithCallbackData("PEN"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("SRD"),
                    InlineKeyboardButton.WithCallbackData("UYU"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("CLP"),

                },
            });
            await _botClient.SendTextMessageAsync(message.From.Id, "Валюты в Южной Америке", replyMarkup: keyboardStep);


            _stateProvider.Add(201,message.From.Id);
        }
        /// <summary>
        /// Метод отправляет пользователю клавиатуру
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        static async Task MiddleEast(Message message)
        {
            if (_stateProvider.ContainsKey(201))
                _stateProvider.Remove(201);

            var keyboardStep = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("BHD"),
                    InlineKeyboardButton.WithCallbackData("BND"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("EGP"),
                    InlineKeyboardButton.WithCallbackData("ILS"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("JOD"),
                    InlineKeyboardButton.WithCallbackData("IQD"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("IRR"),
                    InlineKeyboardButton.WithCallbackData("YER"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("QAR"),
                    InlineKeyboardButton.WithCallbackData("KWD"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("LBP"),
                    InlineKeyboardButton.WithCallbackData("AED"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("OMR"),
                    InlineKeyboardButton.WithCallbackData("ILS"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("SAR"),
                    InlineKeyboardButton.WithCallbackData("SYP"),

                },
            });

          
            await _botClient.SendTextMessageAsync(message.From.Id, "Средний восток", replyMarkup: keyboardStep);

            _stateProvider.Add(201, message.From.Id);
        }
        /// <summary>
        /// Метод отправляет пользователю клавиатуру
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        static async Task AntillesAndCaribeanIslands(Message message)
        {
            if (_stateProvider.ContainsKey(201))
                _stateProvider.Remove(201);

            var keyboardStep = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("XCD"),
                    InlineKeyboardButton.WithCallbackData("AWG"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("BSD"),
                    InlineKeyboardButton.WithCallbackData("BBD"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("BZD"),
                    InlineKeyboardButton.WithCallbackData("BMD"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("HTG"),
                    InlineKeyboardButton.WithCallbackData("DOP"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("CUC"),
                    InlineKeyboardButton.WithCallbackData("ANG"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("KYD"),
                    InlineKeyboardButton.WithCallbackData("JMD"),

                },
            });

            await _botClient.SendTextMessageAsync(message.From.Id, "Валюты на Антийских/Карибских остравах", replyMarkup: keyboardStep);


            _stateProvider.Add(201, message.From.Id);
        }
        /// <summary>
        /// Метод отправляет пользователю клавиатуру
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        static async Task NorthAmerica(Message message)
        {
            if (_stateProvider.ContainsKey(201))
                _stateProvider.Remove(201);

            var keyboardStep = new InlineKeyboardMarkup(new[]
                {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("USD 🇺🇸"),
                    InlineKeyboardButton.WithCallbackData("CAD 🇨🇦"),

                },
            });
            await _botClient.SendTextMessageAsync(message.From.Id, "Валюты в Северной Америке", replyMarkup: keyboardStep);

            _stateProvider.Add(201, message.From.Id);
        }
        /// <summary>
        /// Метод отправляет пользователю клавиатуру
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        static async Task Africa(Message message)
        {
            if (_stateProvider.ContainsKey(201))
                _stateProvider.Remove(201);

            var keyboardStep = new InlineKeyboardMarkup(new[]
              {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("DZD"),
                    InlineKeyboardButton.WithCallbackData("AOA"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("XOF"),
                    InlineKeyboardButton.WithCallbackData("BWP"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("BIF"),
                    InlineKeyboardButton.WithCallbackData("XAF"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("GMD"),
                    InlineKeyboardButton.WithCallbackData("GHS"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("GNF"),
                    InlineKeyboardButton.WithCallbackData("XAF"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("CDF"),
                    InlineKeyboardButton.WithCallbackData("DJF"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("ZMW"),
                    InlineKeyboardButton.WithCallbackData("MAD"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("ZWL"),
                    InlineKeyboardButton.WithCallbackData("CVE"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("KES"),
                    InlineKeyboardButton.WithCallbackData("KMF"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("XOF"),
                    InlineKeyboardButton.WithCallbackData("LSL"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("LRD"),
                    InlineKeyboardButton.WithCallbackData("LYD"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("MUR"),
                    InlineKeyboardButton.WithCallbackData("MRU"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("MGA"),
                    InlineKeyboardButton.WithCallbackData("MAD"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("MZN"),
                    InlineKeyboardButton.WithCallbackData("NAD"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("NGN"),
                    InlineKeyboardButton.WithCallbackData("SHP"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("RWF"),
                    InlineKeyboardButton.WithCallbackData("STN"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("SZL"),
                    InlineKeyboardButton.WithCallbackData("SCR"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("SOS"),
                    InlineKeyboardButton.WithCallbackData("SDG"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("SLL"),
                    InlineKeyboardButton.WithCallbackData("TZS"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("TND"),
                    InlineKeyboardButton.WithCallbackData("UGX"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("XAF"),
                    InlineKeyboardButton.WithCallbackData("ETB"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("ZAR"),
                    InlineKeyboardButton.WithCallbackData("SSP"),

                },
            });
            await _botClient.SendTextMessageAsync(message.From.Id, "Валюты в Африке", replyMarkup: keyboardStep);


            _stateProvider.Add(201, message.From.Id);
        }

        /// <summary>
        /// Метод отправляет пользователю клавиатуру
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        static async Task Europe(Message message)
        {
            if (_stateProvider.ContainsKey(201))
                _stateProvider.Remove(201);

            var keyboardStep = new InlineKeyboardMarkup(new[]
          {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("EUR"),
                    InlineKeyboardButton.WithCallbackData("AZN"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("ALL"),
                    InlineKeyboardButton.WithCallbackData("AMD"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("BYN"),
                    InlineKeyboardButton.WithCallbackData("BGN"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("BAM"),
                    InlineKeyboardButton.WithCallbackData("YUN"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("RUB"),
                    InlineKeyboardButton.WithCallbackData("GBP"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("GIP"),
                    InlineKeyboardButton.WithCallbackData("GEL"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("DKK"),
                    InlineKeyboardButton.WithCallbackData("JEP"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("ISK"),
                    InlineKeyboardButton.WithCallbackData("LTL"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("MDL"),
                    InlineKeyboardButton.WithCallbackData("NOK"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("IMP"),
                    InlineKeyboardButton.WithCallbackData("PLN"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("MKD"),
                    InlineKeyboardButton.WithCallbackData("RON"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("RSD"),
                    InlineKeyboardButton.WithCallbackData("TRY"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("HRK"),
                    InlineKeyboardButton.WithCallbackData("CZK"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("CHF"),
                    InlineKeyboardButton.WithCallbackData("SEK"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("GGP"),

                },
            });

            _stateProvider.Add(201, message.From.Id);

            await _botClient.SendTextMessageAsync(message.From.Id, "Европа", replyMarkup: keyboardStep);
        }

        /// <summary>
        /// Метод отправляет пользователю клавиатуру
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        static async Task Other(Message message)
        {
            if (_stateProvider.ContainsKey(201))
                _stateProvider.Remove(201);

            var keyboardStep = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("AFN"),
                    InlineKeyboardButton.WithCallbackData("BDT"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("BTN"),
                    InlineKeyboardButton.WithCallbackData("VND"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("INR"),
                    InlineKeyboardButton.WithCallbackData("IDR"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("KZT"),
                    InlineKeyboardButton.WithCallbackData("KHR"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("KGS"),
                    InlineKeyboardButton.WithCallbackData("KID"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("CNY"),
                    InlineKeyboardButton.WithCallbackData("LAK"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("MOP"),
                    InlineKeyboardButton.WithCallbackData("MYR"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("MVR"),
                    InlineKeyboardButton.WithCallbackData("MNT"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("MMK"),
                    InlineKeyboardButton.WithCallbackData("NPR"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("PKR"),
                    InlineKeyboardButton.WithCallbackData("PGK"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("KPW"),
                    InlineKeyboardButton.WithCallbackData("SGD"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("TJS"),
                    InlineKeyboardButton.WithCallbackData("TWD"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("THB"),
                    InlineKeyboardButton.WithCallbackData("UZS"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("PHP"),
                    InlineKeyboardButton.WithCallbackData("LKR"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("KRW"),
                    InlineKeyboardButton.WithCallbackData("JPY"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("AUD"),
                    InlineKeyboardButton.WithCallbackData("VUV"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("NZD"),
                    InlineKeyboardButton.WithCallbackData("XPF"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("PAB"),
                    InlineKeyboardButton.WithCallbackData("WST"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("SBD"),
                    InlineKeyboardButton.WithCallbackData("TOP"),

                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("FJD"),

                },
            });
            await _botClient.SendTextMessageAsync(message.From.Id, "Валюты Азии и Тихого океана", replyMarkup: keyboardStep);


            _stateProvider.Add(201, message.From.Id);
        }
        /// <summary>
        /// Метод считывает значение суммы конвертации
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        static (double, string) ReadValOfAmount(Message message)
        {
            // Инициализация переменной
            string tmp;
            // Замена точек на запятые
            tmp = message.Text.Replace(".", ",");
            // проверка, что число имеет не больше 9 нулей
            if (tmp.Length >= 10)
                return (1, "Слишком большое значение");
            // Инициализация переменной
            double output; 
            // парсинг строки
            if (!double.TryParse(tmp, out output))
                return (1, "Вы ввели не корректные данные, повторите попытку");
            // проверка, что число положительное
            if(output<0)
                return (1, "Число не может быть отрицательным");
            // отправка картежа
            return (output, null);
        }
    }
}
