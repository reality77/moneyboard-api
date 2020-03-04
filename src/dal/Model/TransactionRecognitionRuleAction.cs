
using System.Text.Json.Serialization;
using dto;

namespace dal.Model
{
    public partial class TransactionRecognitionRuleAction
    {
        public TransactionRecognitionRuleAction()
        {
        }

        public int Id { get; set; }
        public int TransactionRecognitionRuleId { get; set; }

        public ERecognitionRuleActionType Type { get; set; }
        public string Field { get; set; }
        public string Value { get; set; }

        [JsonIgnore]
        public virtual TransactionRecognitionRule Rule { get; set; }

    }
}
