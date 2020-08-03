using System;

namespace dto.Model
{
    public class TransactionEditRequest
    {
        public DateTime Date { get; set; }
        public DateTime? UserDate { get; set; }
        public string Caption { get; set; }
        public string Comment { get; set; }
    }    
}