using System;
using System.Collections.Generic;

namespace dto.Model
{
    public class AccountDetails : AccountBase
    {
        public decimal InitialBalance { get; set; }

        public AccountDetails()
        {
        }
    }
}
