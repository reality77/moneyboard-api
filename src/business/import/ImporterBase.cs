using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Diagnostics;
using System.IO;

namespace business.import
{
    public abstract class ImporterBase
    {
        public abstract TransactionsFile Import(Stream stream, out List<ImportError> errors);
    }


    public enum ImportFileTypes
    {
        Unknown,
        QIF,
        OFX,
    }
}