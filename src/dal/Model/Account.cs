using System;
using System.Collections.Generic;

namespace dal.Model
{
    public partial class Account
    {
        public Account()
        {
            Transactions = new HashSet<Transaction>();
        }

        public int Id { get; set; }
        public decimal Balance { get; set; }
        public ECurrency Currency { get; set; }
        public decimal InitialBalance { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
