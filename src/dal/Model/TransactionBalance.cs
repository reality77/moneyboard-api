using System;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using dto;

namespace dal.Model
{
    public partial class TransactionBalance
    {
        public TransactionBalance()
        {
        }

        public int Id { get; set; }

        public decimal Balance { get; set; }
    }
}
