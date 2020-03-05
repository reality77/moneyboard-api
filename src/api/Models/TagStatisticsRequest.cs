using System;
using System.Collections.Generic;

namespace api.Models
{
    public class TagStatisticsRequest
    {
        public DateTime? DateStart { get; set; }        
        public DateTime? DateEnd { get; set; }

        public ICollection<int> AccountIds { get; set; }

        public bool IncludeSubTags { get; set; }

        public EDateRange Range { get; set; }

        public TagStatisticsRequest()
        {
            Range = EDateRange.Months;
        }
    } 
    
    public enum EDateRange
    {
        Days,
        Months,
        Years,
    }
}