using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace dal.Model
{
    public partial class ImportedTransaction
    {
        public ImportedTransaction()
        {
        }

        public int Id { get; set; }
        public int FileId { get; set; }
        public decimal Amount { get; set; }
        public string Number { get; set; }
        public string Caption { get; set; }
        public string Comment { get; set; }
        public DateTime Date { get; set; }
        public string Hash { get; set; }

        [JsonIgnore]
        public virtual ImportedFile File { get; set; }
    }
}
