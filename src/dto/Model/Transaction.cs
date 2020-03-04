using System;
using System.Collections.Generic;

namespace dto.Model
{
    public class Transaction
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Caption { get; set; }
        public string Comment { get; set; }
        public DateTime Date { get; set; }
        public ETransactionType Type { get; set; }
        public DateTime? UserDate { get; set; }

        public IEnumerable<Tag> Tags { get; set; }

        public Transaction()
        {
        }
    }
}