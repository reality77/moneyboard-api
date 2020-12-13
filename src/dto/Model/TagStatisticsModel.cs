using System;
using System.Collections.Generic;

namespace dto.Model
{
    public class TagStatisticsModel
    {
        public TagStatisticsModel()
        {
        }

        public int? Year { get; set; }
        public int? Month { get; set; }
        public int? Day { get; set; }
        public decimal Total { get; set; }
        public decimal TagTotal { get; set; }

        public IEnumerable<SingleTagStatistic> SubTagTotals { get; set; }
    }

    public class SingleTagStatistic
    {
        public Tag Tag { get; set; }
        public decimal Amount { get; set; }

    }
}
