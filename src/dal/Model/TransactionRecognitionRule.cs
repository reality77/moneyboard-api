
using System.Collections.Generic;

namespace dal.Model
{
    public partial class TransactionRecognitionRule
    {
        public TransactionRecognitionRule()
        {
            Conditions = new HashSet<TransactionRecognitionRuleCondition>();
            Actions = new HashSet<TransactionRecognitionRuleAction>();
        }

        public int Id { get; set; }

        public bool UseOrConditions { get; set; }

        public virtual ICollection<TransactionRecognitionRuleCondition> Conditions { get; set; }
        public virtual ICollection<TransactionRecognitionRuleAction> Actions { get; set; }
    }
}
