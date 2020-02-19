using System;
using System.Linq;
using System.Collections.Generic;

namespace business
{
    public class ImportError
    {
        public int Line { get; set; }
        public string Error { get; set; }

        // Indique si l'import du fichier ou de la transaction a été skipped
        public bool IsSkipped {get; set; }
        
        public bool IsFatal {get; set; }

        public ImportError()
        {
            IsSkipped = false;
            IsFatal = false;
        }
    }
}