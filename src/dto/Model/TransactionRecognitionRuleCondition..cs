
namespace dto.Model
{
    public partial class TransactionRecognitionRuleCondition
    {
        public TransactionRecognitionRuleCondition()
        {
        }

        public ERecognitionRuleConditionFieldType FieldType { get; set; }
        public string FieldName { get; set; }

        public ERecognitionRuleConditionOperator ValueOperator { get; set; }
        public string Value { get; set; }
    }
}
