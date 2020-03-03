
using System.Text.Json.Serialization;

namespace dal.Model
{
    public partial class TransactionRecognitionRuleCondition
    {
        public TransactionRecognitionRuleCondition()
        {
        }

        public int Id { get; set; }
        public int TransactionRecognitionRuleId { get; set; }

        public ERecognitionRuleConditionFieldType FieldType { get; set; }
        public string FieldName { get; set; }

        public ERecognitionRuleConditionOperator ValueOperator { get; set; }
        public string Value { get; set; }

        [JsonIgnore]
        public virtual TransactionRecognitionRule Rule { get; set; }

    }
}
