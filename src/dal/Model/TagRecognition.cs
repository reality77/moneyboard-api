
using System.Collections.Generic;

namespace dal.Model
{
    public partial class TagRecognition
    {
        public TagRecognition()
        {
        }

        public int Id { get; set; }

        public string RecognizedTagTypeKey { get; set; }
        public string RecognizedTagKey { get; set; }

        public int TargetTagId { get; set; }

        public virtual Tag TargetTag { get; set; }
    }
}
