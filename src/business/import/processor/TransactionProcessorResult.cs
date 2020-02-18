using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Diagnostics;
using System.IO;

namespace business.import.processor
{
    public class TransactionProcessorResult
    {
        public bool SkipTransaction { get; set; }
        public bool StopOtherProcessors { get; set; }
        public IList<ImportError> Errors {get;set;}

        public TransactionProcessorResult()
        {
            Errors = new List<ImportError>();            
        }

    }
}