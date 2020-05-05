using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace JulaFintech
{
    class TradeSet
    {
        public List<Trade> TradesList { get; private set; }
        public TimeSpan TimeSpan { get; set; }

        public TradeSet(TimeSpan timeSpan)
        {
            TradesList = new List<Trade>();
            TimeSpan = timeSpan;
        }
        public void AddTrade(Trade trade)
        {
            TradesList.Add(trade);
            TrimList();
        }
        private void TrimList()
        {
            var TradesToDelete = TradesList.Where(t => t.Date < TradesList.Last().Date - TimeSpan.TotalSeconds).ToList();
            foreach ( Trade trade in TradesToDelete)
            {
                TradesList.Remove(trade);
            }
        }
        public void ClearSet()
        {
            TradesList.Clear();
        }
       
    }
}
