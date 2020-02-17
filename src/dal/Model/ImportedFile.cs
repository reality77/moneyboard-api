using System;
using System.Collections.Generic;

namespace dal.Model
{
    public partial class ImportedFile
    {
        public ImportedFile()
        {
            Transactions = new HashSet<ImportedTransaction>();
        }

        public int Id { get; set; }
        public string FileName { get; set; }
        public DateTime ImportDate { get; set; }
        public virtual ICollection<ImportedTransaction> Transactions { get; set; }
    }
}
