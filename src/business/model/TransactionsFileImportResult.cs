using System;
using System.Linq;
using System.Collections.Generic;

namespace business
{
    public class TransactionsFileImportResult
    {
        public TransactionsFile File { get; set; } 
        public List<ImportError> Errors { get; set; } 
        public int TransactionsDetectedCount { get; set; }
        public int TransactionsImportedCount { get; set; }
        public int TransactionsSkippedCount { get; set; }
        public int TransactionsInErrorCount { get; set; }

        public TransactionsFileImportResult()
        {
            Errors = new List<ImportError>();
        }
    }
}