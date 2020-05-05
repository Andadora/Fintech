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
        public bool IsRisingSlope(double percent, TradeSet tradeSet)
        {
            return tradeSet.TradesList.Any(t => Price - t.Price >= percent / 100 * t.Price);
        }
        public bool IsFallingSlope(double percent, TradeSet tradeSet)
        {
            return tradeSet.TradesList.Any(t => Price - t.Price <= - percent / 100 * t.Price);
        }
    }
}
