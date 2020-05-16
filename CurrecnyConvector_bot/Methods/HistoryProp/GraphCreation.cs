using System;
using System.Collections.Generic;
using System.Drawing;
using ZedGraph;

namespace CurrecnyConvector_bot.Methods.HistoryProp
{
    /// <summary>
    /// Класс для создания графика
    /// </summary>
    public class GraphCreation
    {
        /// <summary>
        /// метод для рисовния графика
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="nameOfCur"></param>
        /// <param name="currencies"></param>
        /// <returns></returns>
        public Bitmap DrawGraph(string from, string to, string nameOfCur, List<(string, string)> currencies)
        {
            // Создание обьекта класса
            GraphPane pane = new GraphPane();
            pane.CurveList.Clear();
            // главная легенда
            pane.Title.Text = $"Динамика курса {nameOfCur} к рублю";
            // леганда на оси абсцисс
            // леганда на оси ординат
            pane.YAxis.Title.Text = "Цена валюты";
            pane.XAxis.Title.Text = "Дата";

            // на оси ординат значения будут в формате даты
            pane.XAxis.Type = AxisType.Date;
            // а именнов таком
            pane.XAxis.Scale.Format = "dd.MM.yyyy";
            // Создание обьекта класса
            PointPairList list = new PointPairList();

            // парсинг строки в DateTime 
            DateTime fromDate = DateTime.Parse(from);
            DateTime toDate = DateTime.Parse(to);

            // подсчет количества дней
            string countOfDays = (toDate - fromDate).ToString();
            int firstDot = countOfDays.IndexOf(".");
            // парсинг строки
            int CountOfDaysInt = int.Parse(countOfDays.Substring(0, firstDot));


            // Создание обьекта класса
            List<XDate> date1 = new List<XDate>();
            // инициализация переменной
            DateTime tmpDate;
            // увелечение на один день
            tmpDate = toDate.AddDays(1);
            // создания списка дат за период
            while (fromDate != tmpDate)
            {
                // добавление даты
                date1.Add(fromDate);
                // увелечение на один
                fromDate = fromDate.AddDays(1);
            }

            // Создание обьекта класса
            TextObj[] tObj = new TextObj[CountOfDaysInt];
            string tmp;
            double val;

           // создание графика по точкам
            for (int i = 0; i < CountOfDaysInt; i++)
            {
                // парсинг строки
                tmp = currencies[i].Item2.Replace(".", ",");

                
                val = double.Parse(tmp);
                // добавление в список
                list.Add(date1[i], val);
            }

            LineItem myCurve1 = pane.AddCurve(nameOfCur, list, Color.Red, SymbolType.Plus);
            myCurve1.Line.Fill = new Fill(Color.Blue, Color.White);


            myCurve1.Line.IsSmooth = true;
            pane.AxisChange();
            
            // превращение граика в точечный рисунок
            var pb = (PaneBase)pane;
            var bm = pb.GetImage();

            // отправка рисунка
            return bm;

        }
    }
}
