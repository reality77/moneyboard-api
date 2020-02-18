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
        public IList<IImportProcessor> Processors;

        public abstract TransactionsFile Import(Stream stream, out List<ImportError> errors);

        public ImporterBase()
        {
            Processors = new List<IImportProcessor>();            
        }
    }

    public enum ImportFileTypes
    {
        Unknown,
        QIF,
        OFX,
    }
}