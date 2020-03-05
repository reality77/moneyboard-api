using System;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using dto;

namespace dal.Model
{
    public partial class Transaction
    {
        public Transaction()
        {
            TransactionTags = new HashSet<TransactionTag>();
        }

        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Caption { get; set; }
        public string Comment { get; set; }
        public DateTime Date { get; set; }
        public ETransactionType Type { get; set; }
        public DateTime? UserDate { get; set; }

        public int AccountId { get; set; }
        
        public virtual Account Account { get; set; }

        public virtual ICollection<TransactionTag> TransactionTags { get; set; }
    }
}
