using System;
using System.Collections.Generic;

namespace dto.Model
{
    public class TagDetails : Tag
    {
        public string ParentKey { get; set; }
        
        public IEnumerable<string> SubKeys { get; set; }
    }
}