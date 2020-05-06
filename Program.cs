using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace JulaFintech
{
    class Program
    {
        public static string data_directory = @"D:\studia\NETiJava\Fintech\DATA\BitBay";
        static void Main()
        {
            DateTime since = new DateTime(2016, 5, 1, 0, 00, 00);
            DateTime to = new DateTime(2017, 5, 1, 0, 00, 00);
            var TradesEnum = GetTradesEnum(since, to);
            foreach (Trade trade in GetFallingSlopesEnum(TradesEnum, 10, TimeSpan.FromHours(1)))
            {
                Console.WriteLine(trade);
            }
        }

        public static IEnumerable<Trade> GetTradesEnum(DateTime since, DateTime to)
        {
            var since_unix = (long)(since.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            var to_unix = (long)(to.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            var since_int = int.Parse(since.ToString("yyyyMMdd"));
            var to_int = int.Parse(to.ToString("yyyyMMdd"));

            var paths = Directory.EnumerateFiles(data_directory, "BTCPLN*.json");
            var filteredpaths = paths.Where(p =>
            {
                var s = p.Split('_');
                return int.Parse(s[s.Length - 2]) >= since_int && int.Parse(s[s.Length - 3]) <= to_int;
            });

            foreach (string path in filteredpaths)
            {
                string jsonString = File.ReadAllText(path);
                var trades = JsonConvert.DeserializeObject<Trade[]>(jsonString);
                foreach (var trade in trades)
                {
                    if (since_unix <= trade.Date && trade.Date <= to_unix)
                    {
                        yield return trade;
                    }
                }
            }
        }
        public static IEnumerable<Trade> GetRisingSlopesEnum(IEnumerable<Trade> trades, double percent, TimeSpan timeSpan)
        {
            var tradeSet = new TradeSetDropWatcher(timeSpan, percent);
            foreach(Trade trade in trades)
            {
                tradeSet.AddTrade(trade);
                if(trade.IsRisingSlope(tradeSet))
                {
                    tradeSet.ClearSet();
                    yield return trade;
                }
            }
        }

        public static IEnumerable<Trade> GetFallingSlopesEnum(IEnumerable<Trade> trades, double percent, TimeSpan timeSpan)
        {
            var tradeSet = new TradeSetDropWatcher(timeSpan, percent);
            foreach (Trade trade in trades)
            {
                tradeSet.AddTrade(trade);
                if (trade.IsFallingSlope(tradeSet))
                {
                    tradeSet.ClearSet();
                    yield return trade;
                }
            }
        }
        public static int[] GetMaxMonth(List<Trade> trades)
        {
            var grouped = trades.ToLookup(t => new DateTime(
                DateTimeOffset.FromUnixTimeSeconds(t.Date).Year,
                DateTimeOffset.FromUnixTimeSeconds(t.Date).Month,
                1));
            var maxGroup = grouped.Aggregate((grp, maxSoFar)
                => maxSoFar == null || grp.Count() > maxSoFar.Count() ? grp : maxSoFar);
            return new int[3] { maxGroup.Key.Year, maxGroup.Key.Month, maxGroup.Count() };
        }
        public static int[] GetMinMonth(List<Trade> trades)
        {
            var grouped = trades.ToLookup(t => new DateTime(
                DateTimeOffset.FromUnixTimeSeconds(t.Date).Year,
                DateTimeOffset.FromUnixTimeSeconds(t.Date).Month,
                1));
            var minGroup = grouped.Aggregate((grp, minSoFar)
                => minSoFar == null || grp.Count() < minSoFar.Count() ? grp : minSoFar);
            return new int[3] { minGroup.Key.Year, minGroup.Key.Month, minGroup.Count() };
        }
    }
}
