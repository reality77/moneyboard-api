using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using dal.Model;
using Microsoft.EntityFrameworkCore;

namespace business.transaction.processor
{
    public class RecognitionRulesProcessor : ITransactionProcessor
    {
        public void ProcessTransaction(MoneyboardContext db, ImportedTransaction transaction)
        {
            var rules = db.TransactionRecognitionRules
                .Include(rr => rr.Conditions)
                .Include(rr => rr.Actions);

            foreach(var rule in rules)
                ExecuteRule(transaction, rule);
        }

        private void ExecuteRule(ImportedTransaction transaction, TransactionRecognitionRule rule)
        {
            bool performActions = false;

            foreach(var condition in rule.Conditions)
            {
                if(IsConditionApplied(transaction, condition))
                    performActions = true;
                else
                {
                    if(!rule.UseOrConditions)
                        return; // en mode AND et au moins une condition non respectée => On sort
                }
            }

            if(performActions)
            {
                // TODO : Perform actions
            }
        }

        private bool IsConditionApplied(ImportedTransaction transaction, TransactionRecognitionRuleCondition condition)
        {
            object valueToTest = null;

            switch(condition.FieldType)
            {
                case ERecognitionRuleConditionFieldType.DataField:
                    {
                        // TODO : Reflection
                        switch(condition.FieldName.ToLower())
                        {
                            case "caption":
                                valueToTest = transaction.Caption;
                                break;
                            case "comment":
                                valueToTest = transaction.Caption;
                                break;
                            case "importcaption":
                                valueToTest = transaction.Comment;
                                break;
                            case "importcomment":
                                valueToTest = transaction.ImportComment;
                                break;
                            default:
                                return false;
                        }
                    }
                    break;
                case ERecognitionRuleConditionFieldType.Tag:
                    {
                        var tags = transaction.TransactionTags.Select(tt => tt.Tag)
                            .Where(t => t.TagTypeKey.ToLower() == condition.FieldName.ToLower())
                            .Where(t => t.Key.ToLower() == "");

                        if(tags.Any(t => t.Key.ToLower() == condition.Value.ToLower()))
                            return true;
                    }
                    break;
            }

            return false;
        }
    }
}