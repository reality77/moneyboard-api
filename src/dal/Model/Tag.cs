using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace dal.Model
{
    public partial class Tag
    {
        public Tag()
        {
            TransactionTags = new HashSet<TransactionTag>();
            SubTags = new HashSet<Tag>();
        }

        public int Id { get; set; }
        public string Key { get; set; }
        public string Caption { get; set; }

        public string TagTypeKey { get; set; }
        
        public virtual TagType Type { get; set; }
        
        public int? ParentTagId { get; set; }
        
        public virtual Tag ParentTag { get; set; }
        
        [JsonIgnore]
        public virtual ICollection<TransactionTag> TransactionTags { get; set; }

        public virtual ICollection<Tag> SubTags { get; set; }
    }
}
