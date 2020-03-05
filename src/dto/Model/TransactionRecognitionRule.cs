
using System.Collections.Generic;

namespace dto.Model
{
    public class TransactionRecognitionRuleBase
    {
        public TransactionRecognitionRuleBase()
        {
            Conditions = new HashSet<TransactionRecognitionRuleCondition>();
            Actions = new HashSet<TransactionRecognitionRuleAction>();
        }

        public bool UseOrConditions { get; set; }

        public IEnumerable<TransactionRecognitionRuleCondition> Conditions { get; set; }
        public IEnumerable<TransactionRecognitionRuleAction> Actions { get; set; }
    }

    public class TransactionRecognitionRule : TransactionRecognitionRuleBase
    {
        public int Id { get; set; }
    }

    public class TransactionRecognitionRuleEdit : TransactionRecognitionRuleBase
    {
    }
}
