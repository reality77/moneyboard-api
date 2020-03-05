using System;
using System.Collections.Generic;

namespace dto.Model
{
    public class ImportedTransaction : Transaction
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
    }
}
