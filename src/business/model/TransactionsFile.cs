using System;
using System.Linq;
using System.Collections.Generic;

namespace business
{
    public class TransactionsFile
    {
        public List<TransactionData> Transactions { get; set; }

        public TransactionsFile()
        {
            Transactions = new List<TransactionData>();
        }
    }
}