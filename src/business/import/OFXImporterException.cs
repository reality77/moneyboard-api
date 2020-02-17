using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace business.import
{
    public class OFXImporterException : Exception
    {
        public OFXImporterException(int lineNo, string message, string field)
            : base("At line " + lineNo + " [" + field + "] : " + message)
        {
        }
    }
}
