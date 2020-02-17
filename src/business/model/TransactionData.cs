using System;
using System.Linq;
using System.Collections.Generic;

namespace business
{
    public class TransactionData
    {
        public string Hash { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Caption { get; set; }
        public string Number { get; set; }
        public string TransferTarget { get; set; }
        public string Category { get; set; }
        public string Error { get; set; }
        public string Memo { get; set; }
    }
}