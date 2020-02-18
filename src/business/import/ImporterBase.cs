using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Diagnostics;
using System.IO;
using business.import.processor;

namespace business.import
{
    public abstract class ImporterBase
    {
        public IList<IImportProcessor> Processors {get; private set; }

        public abstract TransactionsFile Import(string fileName, Stream stream, out List<ImportError> errors);

        public ImporterBase()
        {
            Processors = new List<IImportProcessor>();            
        }

		protected bool SkipFile(TransactionsFile file)
		{
			foreach(IImportProcessor processor in this.Processors)
			{
				if(!processor.ProcessImportedFile(file))
                    return true;
			}

			return false;
		}

		protected TransactionProcessorResult RunTransactionProcessors(TransactionsFile file, TransactionData data)
		{
			var result = new TransactionProcessorResult();

			foreach(IImportProcessor processor in this.Processors)
			{
				processor.ProcessImportedTransaction(file, data, ref result);
			}

			return result;
		}
    }

    public enum ImportFileTypes
    {
        Unknown,
        QIF,
        OFX,
    }
}