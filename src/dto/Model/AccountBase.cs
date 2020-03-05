using System;
using System.Collections.Generic;

namespace dto.Model
{
    public class AccountBase
    {
        public AccountBase()
        {
        }

        public int Id { get; set; }
        public decimal Balance { get; set; }
        public ECurrency Currency { get; set; }
        public string Name { get; set; }
    }
}
