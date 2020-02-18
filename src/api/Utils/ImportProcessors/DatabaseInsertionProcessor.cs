using System;
using System.Collections.Generic;
using System.Linq;
using business;
using business.import.processor;
using dal.Model;

namespace api.Utils.ImportProcessors
{
    public class DatabaseInsertionProcessor : IImportProcessor
    {
        private readonly MoneyboardContext _db;

        private Dictionary<TransactionsFile, ImportedFile> _dicFiles = new Dictionary<TransactionsFile, ImportedFile>();

        public DatabaseInsertionProcessor(MoneyboardContext db)
        {
            _db = db;
        }

        public bool ProcessFile(TransactionsFile file)
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

        public void ProcessTransaction(TransactionsFile file, TransactionData data, ref TransactionProcessorResult result)
        {
            if(_db.ImportedTransactions.Any(t => t.Hash == data.Hash))
            {
                result.Errors.Add(new ImportError() { Error = "Hash already present in datababse" });
                result.SkipTransaction = true;
            }

            var ifile = _dicFiles[file];

            ifile.Transactions.Add(new ImportedTransaction
            {
                Amount = data.Amount,
                Caption = data.Caption,
                Comment = data.Memo,
                Date = data.Date,
                File = ifile,
                Hash = data.Hash,
                Number = data.Number,
            });
        }
    }
}