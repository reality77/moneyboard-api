using System;
using System.Collections.Generic;

namespace dto.Model
{
    public class TagStatisticsModel
    {
        public TagStatisticsModel()
        {
        }

        public int Year { get; set; }
        public int? Month { get; set; }
        public int? Day { get; set; }
        public decimal Total { get; set; }
    }
}
