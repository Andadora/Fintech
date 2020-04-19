using System;
using System.Collections.Generic;
using System.Text;

namespace JulaFintech
{
    class Trade
    {
        public double amount { get; set; }
        public long date { get; set; }
        public double price { get; set; }
        public long tid { get; set; }
        public string type { get; set; }

        public override string ToString() 
        {
            return $"TradeID: {tid}, Amount: {amount}, Date: {date}, Price: {price}, Type: {type}";
        }
    }
}
