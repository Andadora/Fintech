using System;
using System.Linq;
using System.Collections.Generic;

namespace JulaFintech
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime since = new DateTime(2016, 5, 1, 0, 00, 00);
            DateTime to = new DateTime(2016, 6, 1, 0, 00, 00);
            var tradeSet = new TradeSet(since, to);
            var maxMonth = tradeSet.GetMaxMonth();
            Console.WriteLine($"MaxMonth: {maxMonth[0]}.{maxMonth[1]} : {maxMonth[2]}");
            var minMonth = tradeSet.GetMinMonth();
            Console.WriteLine($"MinMonth: {minMonth[0]}.{minMonth[1]} : {minMonth[2]}");
            foreach (Trade trade in tradeSet.GetPeaks(1, 1))
            {
                Console.WriteLine(trade);
            }
        }
    }
}
