using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace dal.Model
{
    public partial class ImportedTransaction : Transaction
    {
        public ImportedTransaction()
         : base()
        {
        }

        public int ImportFileId { get; set; }
        public string ImportNumber { get; set; }
        public string ImportCaption { get; set; }
        public string ImportComment { get; set; }
        public string ImportHash { get; set; }

        [JsonIgnore]
        public virtual ImportedFile ImportFile { get; set; }
    }
}
