using System;
using System.Linq;

namespace JulaFintech
{
    class Trade
    {
        public double Amount { get; set; }
        public long Date { get; set; }
        public double Price { get; set; }
        public long Tid { get; set; }
        public string Type { get; set; }

        public override string ToString() 
        {
            return $"TradeID: {Tid}, Amount: {Amount}, Date: {Date}, Price: {Price}, Type: {Type}";
        }
        public bool IsPeak(double percent, double timeHours, long lastPeakDate)
        {
            var Set = new TradeSet(
                DateTimeOffset.FromUnixTimeSeconds(Date - (long)(3600 * timeHours)).DateTime,
                DateTimeOffset.FromUnixTimeSeconds(Date).DateTime);
            return Set.Trades.Any(t =>
                Date > lastPeakDate + timeHours * 3600
                && Date - t.Date >= 0
                && Date - t.Date <= timeHours * 3600
                && Price - t.Price >= percent / 100 * t.Price);
        }
        public bool IsValley(double percent, double timeHours, long lastValleyDate)
        {
            var Set = new TradeSet(
                DateTimeOffset.FromUnixTimeSeconds(Date - (long)(3600 * timeHours)).DateTime,
                DateTimeOffset.FromUnixTimeSeconds(Date).DateTime);
            return Set.Trades.Any(t =>
                Date > lastValleyDate + timeHours * 3600
                && Date - t.Date >= 0
                && Date - t.Date <= timeHours * 3600
                && Price - t.Price <= - percent / 100 * t.Price);
        }
    }
}
