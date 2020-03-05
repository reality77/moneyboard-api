using System;
using System.Collections.Generic;

namespace dto.Model
{
    public partial class ImportedFile
    {
        public ImportedFile()
        {
        }

        public int Id { get; set; }
        public string FileName { get; set; }
        public DateTime ImportDate { get; set; }
    }
}
