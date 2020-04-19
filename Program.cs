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
            var maxMonth = GetMaxMonth(tradeSet);
            Console.WriteLine($"MaxMonth: {maxMonth[0]}.{maxMonth[1]} : {maxMonth[2]}");
            var minMonth = GetMinMonth(tradeSet);
            Console.WriteLine($"MinMonth: {minMonth[0]}.{minMonth[1]} : {minMonth[2]}");
            foreach (Trade trade in GetPeaks(tradeSet, 1, 1))
            {
                Console.WriteLine(trade);
            }
        }

        static IEnumerable<Trade> GetPeaks(TradeSet tradeSet, double percent, double timeHours)
        {
            var trades = tradeSet.trades;
            var filteredTrades =
                from trade in trades
                where PriceIncreased(trade, tradeSet, percent, timeHours)
                select trade;
            return filteredTrades;
        }

        private static bool PriceIncreased(Trade tradeToCheck, TradeSet tradeSet, double percent, double timeHours)
        {
            foreach (Trade trade in tradeSet.trades.Where(t =>
            tradeToCheck.date - t.date >= 0
            && tradeToCheck.date - t.date <= timeHours * 3600
            && tradeToCheck.price - t.price >= percent / 100 * t.price))
            {
                return true;
            }
            return false;
        }

        static IEnumerable<Trade> GetValleys(TradeSet tradeSet, double percent, double timeHours)
        {
            var trades = tradeSet.trades;
            var filteredTrades =
                from trade in trades
                where PriceDecreased(trade, tradeSet, percent, timeHours)
                select trade;
            return filteredTrades;
        }

        private static bool PriceDecreased(Trade tradeToCheck, TradeSet tradeSet, double percent, double timeHours)
        {
            foreach (Trade trade in tradeSet.trades.Where(t =>
            tradeToCheck.date - t.date >= 0
            && tradeToCheck.date - t.date <= timeHours * 3600
            && tradeToCheck.price - t.price <= - percent / 100 * t.price))
            {
                return true;
            }
            return false;
        }

        static int[] GetMaxMonth(TradeSet tradeSet)
        {
            var trades = tradeSet.trades;
            var grouped = trades.ToLookup(t => new DateTime(
                DateTimeOffset.FromUnixTimeSeconds(t.date).Year,
                DateTimeOffset.FromUnixTimeSeconds(t.date).Month,
                1));
            var maxGroup = grouped.Aggregate((grp, maxSoFar)
                => maxSoFar == null || grp.Count() > maxSoFar.Count() ? grp : maxSoFar);
            return new int[3] {maxGroup.Key.Year, maxGroup.Key.Month, maxGroup.Count()};
        }

        static int[] GetMinMonth(TradeSet tradeSet)
        {
            var trades = tradeSet.trades;
            var grouped = trades.ToLookup(t => new DateTime(
                DateTimeOffset.FromUnixTimeSeconds(t.date).Year,
                DateTimeOffset.FromUnixTimeSeconds(t.date).Month,
                1));
            var minGroup = grouped.Aggregate((grp, minSoFar)
                => minSoFar == null || grp.Count() < minSoFar.Count() ? grp : minSoFar);
            return new int[3] { minGroup.Key.Year, minGroup.Key.Month, minGroup.Count() };
        }
    }
}
