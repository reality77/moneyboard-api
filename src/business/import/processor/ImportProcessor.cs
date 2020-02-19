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
        // Retourner true si le fichier doit être traité, false pour ne pas le traiter
        bool ProcessImportedFile(TransactionsFile file);

        void ProcessImportedTransaction(int line, TransactionsFile file, TransactionData data, ref TransactionProcessorResult result);
    }
}