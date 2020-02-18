using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Diagnostics;
using System.IO;

namespace business.import.processor
{
    public interface IImportProcessor
    {
        void Process(TransactionsFile file, TransactionData data, ref TransactionProcessorResult result);
    }
}