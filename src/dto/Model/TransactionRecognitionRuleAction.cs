

namespace dto.Model
{
    public partial class TransactionRecognitionRuleAction
    {
        public TransactionRecognitionRuleAction()
        {
        }

        public ERecognitionRuleActionType Type { get; set; }
        public string Field { get; set; }
        public string Value { get; set; }
    }
}
