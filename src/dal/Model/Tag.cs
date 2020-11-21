using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

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

        public string TypeKey { get; set; }
        
        public virtual TagType Type { get; set; }
        
        public int? ParentTagId { get; set; }
        
        public virtual Tag ParentTag { get; set; }
        
        [JsonIgnore]
        public virtual ICollection<TransactionTag> TransactionTags { get; set; }

        public virtual ICollection<Tag> SubTags { get; set; }

        public override string ToString() => $"{TypeKey}:{Key}";

        public IEnumerable<Tag> GetAllSubTags(MoneyboardContext context)
        {
            var allTags = context.Tags.AsQueryable()
                .Include(t => t.ParentTag)
                .Include(t => t.SubTags)
                .Where(t => t.TypeKey == this.TypeKey) // on considère que le type de tag ne change pas
                .ToList();

            return RetrieveSubTags(allTags, this).AsReadOnly();
        }

        private List<Tag> RetrieveSubTags(IEnumerable<Tag> allTags, Tag parentTag)
        {
            var detectedTags = new List<Tag>();

            if(parentTag.SubTags.Count() == 0)
                return detectedTags;
            else
            {
                foreach(var subtag in parentTag.SubTags)
                {
                    detectedTags.Add(subtag);
                    detectedTags.AddRange(RetrieveSubTags(allTags, subtag));
                }

                return detectedTags; 
            }
        }
    }
}
