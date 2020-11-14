using System;

namespace dto.Model
{
    public class TransactionTag
    {
        public string TagTypeKey { get; set; }
        public string TagKey { get; set; }
        public string TagCaption { get; set; }

        public bool IsManual { get; set; }
    }
}