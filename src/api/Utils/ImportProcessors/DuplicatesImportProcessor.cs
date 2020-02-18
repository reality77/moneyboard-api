using business;
using business.import.processor;

namespace api.Utils.ImportProcessors
{
    public class DuplicatesImportProcessor : IImportProcessor
    {
        public bool ProcessFile(TransactionsFile file) => true;

        public void ProcessTransaction(TransactionsFile file, TransactionData data, ref TransactionProcessorResult result)
        {
            // TODO
        }
    }
}