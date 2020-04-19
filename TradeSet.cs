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

        static IEnumerable<Trade> GetTradesEnum(DateTime since, DateTime to)
        {
            string data_directory = @"D:\studia\NETiJava\Fintech\DATA\BitBay";
            var since_unix = (long)(since.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            var to_unix = (long)(to.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            var since_int = int.Parse(since.ToString("yyyyMMdd"));
            var to_int = int.Parse(to.ToString("yyyyMMdd"));

            var paths = Directory.EnumerateFiles(data_directory, "BTCPLN*.json");
            foreach (string path in paths)
            {
                int start = int.Parse(path.Split('_')[1]);
                int stop = int.Parse(path.Split('_')[2]);
                if (stop >= since_int && start <= to_int)
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
}
