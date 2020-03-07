using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using dal.Model;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Microsoft.Extensions.Logging;
using dto;

namespace business.transaction.processor
{
    public class RecognitionRulesProcessor : ITransactionProcessor
    {
        private readonly ILogger<RecognitionRulesProcessor> _logger;

        public RecognitionRulesProcessor(ILogger<RecognitionRulesProcessor> logger)
        {
            _logger = logger;
        }

        public void ProcessTransaction(MoneyboardContext db, ImportedTransaction transaction)
        {
            var rules = db.TransactionRecognitionRules
                .Include(rr => rr.Conditions)
                .Include(rr => rr.Actions);

            foreach(var rule in rules.ToList())
            {
                _logger.LogDebug($"Executing rule #{rule.Id} for transaction {transaction.ImportHash}");
                ExecuteRule(transaction, rule, db);
            }
        }

        private void ExecuteRule(ImportedTransaction transaction, TransactionRecognitionRule rule, MoneyboardContext db)
        {
            bool performActions = false;

            foreach(var condition in rule.Conditions.ToList())
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
                foreach(var action in rule.Actions.ToList())
                    PerformAction(transaction, action, db);

                db.SaveChanges();
            }
        }

        private bool IsConditionApplied(ImportedTransaction transaction, TransactionRecognitionRuleCondition condition)
        {
            object valueToTest = null;

            switch(condition.FieldType)
            {
                case ERecognitionRuleConditionFieldType.DataField:
                    {
                        var property = GetProperty(condition.FieldName);

                        if(property == null)
                        {
                            _logger.LogWarning($"Condition #{condition.Rule.Id}.{condition.Id} : BAD_CONFIG : Property {condition.FieldName} not found");
                            return false;
                        }

                        valueToTest = property.GetValue(transaction);

                        object convertedValue = Convert.ChangeType(condition.Value, property.PropertyType, CultureInfo.InvariantCulture);

                        // TODO : Autres cas et conversion type
                        switch(condition.ValueOperator)
                        {
                            case ERecognitionRuleConditionOperator.Equals:
                            {
                                var result = valueToTest.Equals(convertedValue);
                                _logger.LogDebug($"Testing #{condition.Rule.Id}.{condition.Id} : '{Convert.ToString(valueToTest)}' = '{Convert.ToString(convertedValue)}' ?");
                                if(result)
                                    _logger.LogInformation($"Condition #{condition.Rule.Id}.{condition.Id} : MATCH : Equals matched with transaction {transaction.ImportHash}");
                                else
                                    _logger.LogInformation($"Condition #{condition.Rule.Id}.{condition.Id} : NO_MATCH : Equals matched with transaction {transaction.ImportHash}");
                                return result;
                            }
                            case ERecognitionRuleConditionOperator.Contains:
                            {
                                if(property.PropertyType == typeof(string))
                                {
                                    var result = ((string)valueToTest).ToLower().Contains(((string)convertedValue).ToLower());
                                    _logger.LogDebug($"Testing #{condition.Rule.Id}.{condition.Id} : '{(string)valueToTest}' contains '{(string)convertedValue}' ?");
                                    if(result)
                                        _logger.LogInformation($"Condition #{condition.Rule.Id}.{condition.Id} : MATCH : Contains matched with transaction {transaction.ImportHash}");
                                    else
                                        _logger.LogInformation($"Condition #{condition.Rule.Id}.{condition.Id} : NO_MATCH : Contains did not matched with transaction {transaction.ImportHash}");

                                    return result;
                                }
                                else
                                {
                                    _logger.LogInformation($"Condition #{condition.Rule.Id}.{condition.Id} : BAD_CONFIG : Bad type for Contains with transaction {transaction.ImportHash}");
                                    return false;
                                }
                            }
                            case ERecognitionRuleConditionOperator.Greater:
                            case ERecognitionRuleConditionOperator.GreaterOrEquals:
                            case ERecognitionRuleConditionOperator.Lower:
                            case ERecognitionRuleConditionOperator.LowerOrEquals:
                            {
                                if(!property.PropertyType.IsNumeric())
                                {
                                    _logger.LogWarning($"Condition #{condition.Rule.Id}.{condition.Id} : BAD_TYPE : Property {condition.FieldName} is not numeric");
                                    return false;
                                }
                                else
                                {
                                    var comparison = ((IComparable)valueToTest).CompareTo(convertedValue);
                                    bool result = false;
                                    _logger.LogDebug($"Testing #{condition.Rule.Id}.{condition.Id} : Comparing '{Convert.ToString(valueToTest)}' and '{Convert.ToString(convertedValue)}' : Value : {comparison}");

                                    switch(condition.ValueOperator)
                                    {
                                        case ERecognitionRuleConditionOperator.Greater:
                                            result = (comparison > 0);
                                            break;
                                        case ERecognitionRuleConditionOperator.GreaterOrEquals:
                                            result = (comparison >= 0);
                                            break;
                                        case ERecognitionRuleConditionOperator.Lower:
                                            result = (comparison < 0);
                                            break;
                                        case ERecognitionRuleConditionOperator.LowerOrEquals:
                                            result = (comparison <= 0);
                                            break;
                                    }

                                    if(result)
                                        _logger.LogInformation($"Condition #{condition.Rule.Id}.{condition.Id} : MATCH : Equals matched with transaction {transaction.ImportHash}");
                                    else
                                        _logger.LogInformation($"Condition #{condition.Rule.Id}.{condition.Id} : NO_MATCH : Equals matched with transaction {transaction.ImportHash}");
                                    return result;
                                }
                            }
                        }
                    }
                    break;
                case ERecognitionRuleConditionFieldType.Tag:
                    {
                        var tags = transaction.TransactionTags.Select(tt => tt.Tag)
                            .Where(t => t.TypeKey.ToLower() == condition.FieldName.ToLower())
                            .Where(t => t.Key.ToLower() == condition.Value.ToLower());

                        var result = tags.Any();

                        if(result)
                            _logger.LogInformation($"Condition #{condition.Rule.Id}.{condition.Id} : MATCH : Tag matched with transaction {transaction.ImportHash}");
                        else
                            _logger.LogInformation($"Condition #{condition.Rule.Id}.{condition.Id} : NO_MATCH : Tag did not matched with transaction {transaction.ImportHash}");

                        return result;
                    }
            }

            _logger.LogWarning($"Condition #{condition.Rule.Id}.{condition.Id} : BAD_CONFIG : No condition tested with transaction {transaction.ImportHash}");
            return false;
        }
    
        private bool PerformAction(ImportedTransaction transaction, TransactionRecognitionRuleAction action, MoneyboardContext db)
        {
            switch(action.Type)
            {
                case ERecognitionRuleActionType.AddTag:
                {
                    var tag = db.Tags.SingleOrDefault(t => t.TypeKey == action.Field && t.Key == action.Value);

                    if(tag == null)
                    {
                        tag = new Tag { TypeKey = action.Field, Key = action.Value };
                        db.Tags.Add(tag);
                    }

                    transaction.TransactionTags.Add(new TransactionTag { Transaction = transaction, Tag = tag });

                    _logger.LogInformation($"Action #{action.Rule.Id}.{action.Id} : PROCESSED : AddTag {action.Field}:{action.Value} processed for transaction {transaction.ImportHash}");
                    return true;
                }
                case ERecognitionRuleActionType.SetData:
                {
                    var property = GetProperty(action.Field);

                    if(property == null)
                    {
                        _logger.LogWarning($"Action #{action.Rule.Id}.{action.Id} : BAD_CONFIG : Property {action.Field} not found");
                        return false;
                    }

                    property.SetValue(transaction, Convert.ChangeType(action.Value, property.PropertyType));
                    _logger.LogInformation($"Action #{action.Rule.Id}.{action.Id} : PROCESSED : SetData {action.Field} processed for transaction {transaction.ImportHash}");
                    return true;
                }
            }

            return false;
        }

        private PropertyInfo GetProperty(string name)
        {
            Type type = typeof(ImportedTransaction);
            return type.GetProperty(name);
        }
    }
}