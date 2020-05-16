using System;
using System.Collections.Generic;

namespace dto.Model
{
    public class TransactionWithBalance : ImportedTransaction
    {
        public decimal Balance { get; set; }

        public TransactionWithBalance()
        {
        }
    }
}