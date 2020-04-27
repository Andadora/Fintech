using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace JulaFintech
{
    class TradeSet
    {
        public IEnumerable<Trade> Trades { get; }
        public DateTime Since { get; }
        public DateTime To { get; }

        public TradeSet(DateTime since, DateTime to)
        {
            this.Since = since;
            this.To = to;
            this.Trades = GetTradesEnum(since, to);
        }

        public IEnumerable<Trade> GetPeaks(double percent, double timeHours)
        {
            long lastPeakDate = 0;
            var filteredTrades =
                from trade in Trades
                .Where(t =>
                {
                    if (t.IsPeak(percent, timeHours, lastPeakDate))
                    {
                        lastPeakDate = t.Date;
                        return true;
                    }
                    return false;
                })
                select trade;
            return filteredTrades;
        }

        public IEnumerable<Trade> GetValleys(double percent, double timeHours)
        {
            long lastValleyDate = 0;
            var filteredTrades =
                from trade in Trades
                .Where(t => 
                    {
                        if (t.IsValley(percent, timeHours, lastValleyDate))
                        {
                            lastValleyDate = t.Date;
                            return true;
                        }
                        return false;
                    })
                select trade;
            return filteredTrades;
        }
        public int[] GetMaxMonth()
        {
            var grouped = Trades.ToLookup(t => new DateTime(
                DateTimeOffset.FromUnixTimeSeconds(t.Date).Year,
                DateTimeOffset.FromUnixTimeSeconds(t.Date).Month,
                1));
            var maxGroup = grouped.Aggregate((grp, maxSoFar)
                => maxSoFar == null || grp.Count() > maxSoFar.Count() ? grp : maxSoFar);
            return new int[3] { maxGroup.Key.Year, maxGroup.Key.Month, maxGroup.Count() };
        }
        public int[] GetMinMonth()
        {
            var grouped = Trades.ToLookup(t => new DateTime(
                DateTimeOffset.FromUnixTimeSeconds(t.Date).Year,
                DateTimeOffset.FromUnixTimeSeconds(t.Date).Month,
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
                    Trade trade = JsonConvert.DeserializeObject<Trade>(Json);
                    if (since_unix <= trade.Date && trade.Date <= to_unix)
                    {
                        yield return trade;
                    }
                }
            }
        }
    }
}
