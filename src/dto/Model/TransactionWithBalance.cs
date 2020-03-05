using System;
using System.Collections.Generic;

namespace dto.Model
{
    public class TransactionWithBalance : Transaction
    {
        public decimal Balance { get; set; }

        public TransactionWithBalance()
        {
        }
    }
}