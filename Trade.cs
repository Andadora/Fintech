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

        public Trade(double Amount, long Date, double Price, string Type)
        {
            this.Amount = Amount;
            this.Date = Date;
            this.Price = Price;
            this.Type = Type;
        }
        public override string ToString() 
        {
            return $"TradeID: {Tid}, Amount: {Amount}, Date: {DateTimeOffset.FromUnixTimeSeconds(Date).UtcDateTime}, Price: {Price}, Type: {Type}";
        }
        public bool IsRisingSlope(TradeSetDropWatcher tradeSet)
        {
            return tradeSet.TradesList.Any(t => Price - t.Price >= tradeSet.ProcentDivHundred * t.Price);
        }
        public bool IsFallingSlope(TradeSetDropWatcher tradeSet)
        {
            return tradeSet.TradesList.Any(t => Price - t.Price <= - tradeSet.ProcentDivHundred * t.Price);
        }
    }
}
