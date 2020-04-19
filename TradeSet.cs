using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;

namespace JulaFintech
{
    class TradeSet
    {
        public IEnumerable<Trade> trades { get; }
        public DateTime since { get; }
        public DateTime to { get; }

        public TradeSet(DateTime since, DateTime to)
        {
            this.since = since;
            this.to = to;
            this.trades = GetTradesEnum(since, to);
        }

        public IEnumerable<Trade> GetPeaks(double percent, double timeHours)
        {
            var filteredTrades =
                from trade in trades
                where trades.Any(t =>
                            trade.date - t.date >= 0
                            && trade.date - t.date <= timeHours * 3600
                            && trade.price - t.price >= percent / 100 * t.price)
                select trade;
            return filteredTrades;
        }
        public IEnumerable<Trade> GetValleys(double percent, double timeHours)
        {
            var filteredTrades =
                from trade in trades
                where trades.Any(t =>
                            trade.date - t.date >= 0
                            && trade.date - t.date <= timeHours * 3600
                            && trade.price - t.price <= -percent / 100 * t.price)
                select trade;
            return filteredTrades;
        }
        public int[] GetMaxMonth()
        {
            var grouped = trades.ToLookup(t => new DateTime(
                DateTimeOffset.FromUnixTimeSeconds(t.date).Year,
                DateTimeOffset.FromUnixTimeSeconds(t.date).Month,
                1));
            var maxGroup = grouped.Aggregate((grp, maxSoFar)
                => maxSoFar == null || grp.Count() > maxSoFar.Count() ? grp : maxSoFar);
            return new int[3] { maxGroup.Key.Year, maxGroup.Key.Month, maxGroup.Count() };
        }
        public int[] GetMinMonth()
        {
            var grouped = trades.ToLookup(t => new DateTime(
                DateTimeOffset.FromUnixTimeSeconds(t.date).Year,
                DateTimeOffset.FromUnixTimeSeconds(t.date).Month,
                1));
            var minGroup = grouped.Aggregate((grp, minSoFar)
                => minSoFar == null || grp.Count() < minSoFar.Count() ? grp : minSoFar);
            return new int[3] { minGroup.Key.Year, minGroup.Key.Month, minGroup.Count() };
        }
        public IEnumerable<Trade> GetTradesEnum(DateTime since, DateTime to)
        {
            string data_directory = @"D:\studia\NETiJava\Fintech\DATA\BitBay";
            var since_unix = (long)(since.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            var to_unix = (long)(to.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            var since_int = int.Parse(since.ToString("yyyyMMdd"));
            var to_int = int.Parse(to.ToString("yyyyMMdd"));

            var paths = Directory.EnumerateFiles(data_directory, "BTCPLN*.json");
            var filteredpaths = paths.Where(p => 
                int.Parse(p.Split('_')[2]) >= since_int 
                && int.Parse(p.Split('_')[1]) <= to_int);
            foreach (string path in filteredpaths)
            {
                string jsonString = File.ReadAllText(path);
                var split_jsonString = jsonString.Split("},{");
                split_jsonString[0] = split_jsonString[0].TrimStart('[');
                split_jsonString[0] = split_jsonString[0].TrimStart('{');
                split_jsonString[split_jsonString.Length - 1] = split_jsonString[0].TrimStart(']');
                split_jsonString[split_jsonString.Length - 1] = split_jsonString[0].TrimStart('}');
                foreach (string singleJson in split_jsonString)
                {
                    string Json = $"{{{singleJson}}}";
                    Trade trade = JsonSerializer.Deserialize<Trade>(Json);
                    if (since_unix <= trade.date && trade.date <= to_unix)
                    {
                        yield return trade;
                    }
                }
            }
        }
    }
}
