using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace JulaFintech
{
    class TradeSetDropWatcher
    {
        public double ProcentDivHundred { get; set; }
        public List<Trade> TradesList { get; private set; }
        public TimeSpan TimeSpan { get; set; }

        public TradeSetDropWatcher(TimeSpan timeSpan, double procent)
        {
            TradesList = new List<Trade>();
            TimeSpan = timeSpan;
            ProcentDivHundred = procent / 100;
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
