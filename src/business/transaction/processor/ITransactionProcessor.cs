using dal.Model;

namespace business.transaction.processor
{
    public interface ITransactionProcessor
    {
        void ProcessTransaction(MoneyboardContext db, ImportedTransaction transaction);
    }
}