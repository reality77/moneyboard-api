using System;
using System.Text.Json.Serialization;

namespace dal.Model
{
    public partial class TransactionTag
    {
        public TransactionTag()
        {
        }

        public int TagId { get; set; }
        public int TransactionId { get; set; }

        public virtual Tag Tag { get; set; }

        [JsonIgnore]
        public virtual Transaction Transaction { get; set; }
    }
}
