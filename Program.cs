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
            DateTime since = new DateTime(2016, 1, 1, 0, 00, 00);
            DateTime to = new DateTime(2017, 1, 1, 0, 00, 00);
            var TradesEnum = GetTradesEnum(since, to);
            RunStrategy(TradesEnum,
                        10,
                        TimeSpan.FromHours(1),
                        30,
                        30);
        }
        public static void RunStrategy(
            IEnumerable<Trade> trades, 
            double buyPercent, 
            TimeSpan timeSpan, 
            double takeProfitPercent,
            double stopLossPercent)
        {
            var TradeSet = new TradeSetDropWatcher(timeSpan, buyPercent);
            var BuyTradesList = new List<Trade>();
            var toDelete = new List<Trade>();
            double budget = 100.0;
            double profit = 0;
            var currentTrade = new Trade(0, 0, 0, "");
            Console.WriteLine($"|Action:                " + 
                              $"|Date:               " +
                              $"|Type: " +
                              $"|Price: " +
                              $"|Amount: " +
                              $"|Profit/loss: " +
                              $"|Total profit: |");
            foreach (Trade trade in trades)
            {
                foreach(Trade buyTrade in BuyTradesList)
                {
                    if(trade.Price - buyTrade.Price > buyTrade.Price * takeProfitPercent / 100)
                    {
                        profit += trade.Price * buyTrade.Amount;
                        Console.WriteLine($"|Selling at {takeProfitPercent}% rise    " +
                                          $"|{DateTimeOffset.FromUnixTimeSeconds(trade.Date).UtcDateTime} " +
                                          $"|sell  " +
                                          $"|{trade.Price,-7}" +
                                          $"|{Math.Round(buyTrade.Amount, 4),-8}" +
                                          $"|{Math.Round(trade.Price * buyTrade.Amount, 4),-13}" +
                                          $"|{Math.Round(profit, 4), -10}    |");
                        toDelete.Add(buyTrade);
                    }
                    if (buyTrade.Price - trade.Price > buyTrade.Price * stopLossPercent / 100)
                    {
                        profit += trade.Price * buyTrade.Amount;
                        Console.WriteLine($"|Stop loss at {stopLossPercent}% drop     " +
                                          $"|{DateTimeOffset.FromUnixTimeSeconds(trade.Date).UtcDateTime} " +
                                          $"|sell  " +
                                          $"|{trade.Price,-7}" +
                                          $"|{Math.Round(buyTrade.Amount, 4),-8}" +
                                          $"|{Math.Round(trade.Price * buyTrade.Amount, 4),-13}" +
                                          $"|{Math.Round(profit, 4),-10}    |");
                        toDelete.Add(buyTrade);
                    }
                }
                foreach (Trade tradeToDelete in toDelete)
                {
                    BuyTradesList.Remove(tradeToDelete);
                }
                TradeSet.AddTrade(trade);
                if (trade.IsFallingSlope(TradeSet))
                {
                    TradeSet.ClearSet();
                    var buyTrade = new Trade(budget / trade.Price, trade.Date, trade.Price, "buy");
                    BuyTradesList.Add(buyTrade);
                    profit -= budget;
                    Console.WriteLine($"|Buying at {buyPercent}% drop     " +
                                      $"|{DateTimeOffset.FromUnixTimeSeconds(trade.Date).UtcDateTime} " +
                                      $"|buy   " +
                                      $"|{trade.Price,-7}" +
                                      $"|{Math.Round(buyTrade.Amount, 4),-8}" +
                                      $"|{Math.Round(-budget, 4),-13}" +
                                      $"|{Math.Round(profit, 4),-10}    |");
                }
                toDelete.Clear();
                currentTrade = trade;
            }
            if (BuyTradesList.Count() != 0)
            {
                foreach (Trade buyTrade in BuyTradesList)
                {
                    profit += currentTrade.Price * buyTrade.Amount;
                    Console.WriteLine($"|Selling remaining trade" +
                                      $"|{DateTimeOffset.FromUnixTimeSeconds(currentTrade.Date).UtcDateTime} " +
                                      $"|sell  " +
                                      $"|{currentTrade.Price,-7}" +
                                      $"|{Math.Round(buyTrade.Amount, 4),-8}" +
                                      $"|{Math.Round(currentTrade.Price * buyTrade.Amount, 4),-13}" +
                                      $"|{Math.Round(profit, 4),-10}    |");
                }
            }
            Console.WriteLine($"Total profit of strategy {profit}");
            Console.Write("Press enter to exit");
            var _ = Console.ReadLine();
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
