using System;
using System.Linq;
using System.Collections.Generic;

namespace business
{
    public class ImportError
    {
        public int Line { get; set; }
        public string Error { get; set; }
    }
}