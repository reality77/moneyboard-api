using System;
using System.Collections.Generic;
using System.Linq;
using business.transaction.processor;
using dal.Model;

namespace business.import.processor
{
    public class DatabaseInsertionProcessor : IImportProcessor
    {
        private readonly MoneyboardContext _db;
        private readonly Account _account;

        private readonly IEnumerable<ITransactionProcessor> _transactionProcessors;

        private Dictionary<TransactionsFile, ImportedFile> _dicFiles = new Dictionary<TransactionsFile, ImportedFile>();

        public DatabaseInsertionProcessor(MoneyboardContext db, Account account, IEnumerable<ITransactionProcessor> transactionProcessors)
        {
            _db = db;
            _account = account;
            _transactionProcessors = transactionProcessors;
        }

        public bool ProcessImportedFile(TransactionsFile file)
        {
            if(_db.ImportedFiles.Any(f => f.FileName.ToLower() == file.FileName.ToLower()))
                return false; // fichier déjà traité

            var ifile = new ImportedFile()
            {
                FileName = file.FileName,
                ImportDate = DateTime.Now,
            };

            _db.ImportedFiles.Add(ifile);
            _db.SaveChanges();

            _dicFiles.Add(file, ifile);
            return true;
        }

        public void ProcessImportedTransaction(TransactionsFile file, TransactionData data, ref TransactionProcessorResult result)
        {
            if(_db.ImportedTransactions.Any(t => t.ImportHash == data.Hash))
            {
                result.Errors.Add(new ImportError() { Error = "Hash already present in database" });
                result.SkipTransaction = true;
            }

            var ifile = _dicFiles[file];

            var transaction = new ImportedTransaction
            {
                AccountId = _account.Id,
                Date = data.Date,
                Amount = data.Amount,
                ImportFile = ifile,
                ImportCaption = data.Caption,
                ImportComment = data.Memo,
                ImportHash = data.Hash,
                ImportNumber = data.Number,
            };

            foreach(var processor in _transactionProcessors)
                processor.ProcessTransaction(_db, transaction);

            _db.Transactions.Add(transaction);
        }
    }
}