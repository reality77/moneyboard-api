using System;
using System.Linq;
using System.Collections.Generic;

namespace business
{
    public class TransactionsFile
    {
        public string FileName { get; set; }

        public List<TransactionData> Transactions { get; set; }

        public TransactionsFile()
        {
            Transactions = new List<TransactionData>();
        }
    }
}