using System;
using System.Collections.Generic;

namespace dal.Model
{
    public partial class TagType
    {
        public TagType()
        {
            Tags = new HashSet<Tag>();
        }

        public string Key { get; set; }
        public string Caption { get; set; }

        /// <summary>
        /// Indique qu'un seul tag de ce type est autorisé dans une transaction
        /// </summary>
        /// <value></value>
        public bool OneTagOnly { get; set; }

        public virtual ICollection<Tag> Tags { get; set; }
    }
}
