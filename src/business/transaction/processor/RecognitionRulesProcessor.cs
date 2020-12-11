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
using System.Threading.Tasks;

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

                        // TODO : Autres cas et conversion type
                        switch(condition.ValueOperator)
                        {
                            case ERecognitionRuleConditionOperator.Equals:
                            {
                                object convertedValue = Convert.ChangeType(condition.Value, property.PropertyType, CultureInfo.InvariantCulture);
                                var result = valueToTest.Equals(convertedValue);
                                _logger.LogDebug($"Testing #{condition.Rule.Id}.{condition.Id} : '{Convert.ToString(valueToTest)}' = '{Convert.ToString(convertedValue)}' ?");
                                if(result)
                                    _logger.LogInformation($"Condition #{condition.Rule.Id}.{condition.Id} : MATCH : Equals matched with transaction {transaction.ImportHash}");
                                else
                                    _logger.LogDebug($"Condition #{condition.Rule.Id}.{condition.Id} : NO_MATCH : Equals matched with transaction {transaction.ImportHash}");
                                return result;
                            }
                            case ERecognitionRuleConditionOperator.Contains:
                            {
                                if(property.PropertyType == typeof(string))
                                {
                                    var result = ((string)valueToTest).ToLower().Contains(condition.Value.ToLower());
                                    _logger.LogDebug($"Testing #{condition.Rule.Id}.{condition.Id} : '{(string)valueToTest}' contains '{condition.Value}' ?");
                                    if(result)
                                        _logger.LogInformation($"Condition #{condition.Rule.Id}.{condition.Id} : MATCH : Contains matched with transaction {transaction.ImportHash}");
                                    else
                                        _logger.LogDebug($"Condition #{condition.Rule.Id}.{condition.Id} : NO_MATCH : Contains did not matched with transaction {transaction.ImportHash}");

                                    return result;
                                }
                                else
                                {
                                    _logger.LogWarning($"Condition #{condition.Rule.Id}.{condition.Id} : BAD_CONFIG : Bad type for Contains with transaction {transaction.ImportHash}");
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
                                    object convertedValue = Convert.ChangeType(condition.Value, property.PropertyType, CultureInfo.InvariantCulture);

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
                                        _logger.LogInformation($"Condition #{condition.Rule.Id}.{condition.Id} : MATCH : {condition.ValueOperator} matched with transaction {transaction.ImportHash}");
                                    else
                                        _logger.LogDebug($"Condition #{condition.Rule.Id}.{condition.Id} : NO_MATCH : {condition.ValueOperator} matched with transaction {transaction.ImportHash}");
                                    return result;
                                }
                            }
                            case ERecognitionRuleConditionOperator.DayEquals:
                            case ERecognitionRuleConditionOperator.WeekEquals:
                            case ERecognitionRuleConditionOperator.MonthEquals:
                            case ERecognitionRuleConditionOperator.YearEquals:
                            case ERecognitionRuleConditionOperator.DayNear:
                            case ERecognitionRuleConditionOperator.DayOfWeekEquals:
                            {
                                if(property.PropertyType != typeof(DateTime))
                                {
                                    _logger.LogWarning($"Condition #{condition.Rule.Id}.{condition.Id} : BAD_TYPE : Property {condition.FieldName} is not a date");
                                    return false;
                                }
                                else
                                {
                                    bool result = false;

                                    Calendar calendar = new GregorianCalendar();
                                    int val = Convert.ToInt32(condition.Value);
                                    DateTime dateToTest = (DateTime)valueToTest;
                                    _logger.LogDebug($"Testing #{condition.Rule.Id}.{condition.Id} : Comparing '{dateToTest}' and '{val}' with Operator : {condition.ValueOperator}");

                                    switch(condition.ValueOperator)
                                    {
                                        case ERecognitionRuleConditionOperator.DayEquals:
                                            result = (dateToTest.Day == val);
                                            break;
                                        case ERecognitionRuleConditionOperator.DayNear:
                                            result = (Math.Abs(dateToTest.Day - val) <= 3);
                                            break;
                                        case ERecognitionRuleConditionOperator.DayOfWeekEquals:
                                            result = (dateToTest.DayOfWeek == (DayOfWeek)val);
                                            break;
                                        case ERecognitionRuleConditionOperator.WeekEquals:
                                            result = (calendar.GetWeekOfYear(dateToTest, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Friday) == val);
                                            break;
                                        case ERecognitionRuleConditionOperator.MonthEquals:
                                            result = (dateToTest.Month == val);
                                            break;
                                        case ERecognitionRuleConditionOperator.YearEquals:
                                            result = (dateToTest.Year == val);
                                            break;
                                    }

                                    if(result)
                                        _logger.LogInformation($"Condition #{condition.Rule.Id}.{condition.Id} : MATCH : {condition.ValueOperator} matched with transaction {transaction.ImportHash}");
                                    else
                                        _logger.LogDebug($"Condition #{condition.Rule.Id}.{condition.Id} : NO_MATCH : {condition.ValueOperator} matched with transaction {transaction.ImportHash}");
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
                            _logger.LogDebug($"Condition #{condition.Rule.Id}.{condition.Id} : NO_MATCH : Tag did not matched with transaction {transaction.ImportHash}");

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
                    var tagtype = db.TagTypes.SingleOrDefault(t => t.Key == action.Field);

                    if(tagtype == null)
                    {
                        _logger.LogWarning($"Action #{action.Rule.Id}.{action.Id} : BAD_CONFIG : Tag type {action.Field} not found");
                        return false;
                    }

                    // Si un tag de ce type existe déjà dans la transaction, 
                    var existingTransactionTagOfType = transaction.TransactionTags.SingleOrDefault(t => t.Tag.TypeKey == action.Field);

                    if(existingTransactionTagOfType != null)
                    {
                        if(existingTransactionTagOfType.IsManual)
                        {
                            // Si tag existant est manuel, on ne peut pas en ajouter de nouveau
                            _logger.LogInformation($"Action #{action.Rule.Id}.{action.Id} : NOT PROCESSED : AddTag {action.Field}:{action.Value} not processed for transaction {transaction.ImportHash} : Tag of type {action.Field} already exists in transaction");
                            return true;
                        }
                        else
                        {
                            // Si tag existant est automatique, on remplace l'existant
                            _logger.LogDebug($"Action #{action.Rule.Id}.{action.Id} : AddTag {action.Field}:{action.Value} for transaction {transaction.ImportHash} : REPLACING tag {existingTransactionTagOfType.Tag.Key} of type {action.Field}");
                            transaction.TransactionTags.Remove(existingTransactionTagOfType);
                        }
                    }

                    var tag = db.Tags.SingleOrDefault(t => t.TypeKey == action.Field && t.Key == action.Value);

                    // Si le tag n'existe pas, on le créé
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

        public async Task<IQueryable<ImportedTransaction>> FindApplicableTransactionsInRuleAsync(dal.Model.MoneyboardContext db, dto.Model.TransactionRecognitionRuleBase rule)
        {
            var transactions = db.ImportedTransactions
                .Include(t => t.TransactionTags)
                .ThenInclude(tt => tt.Tag)
                .ThenInclude(t => t.Type)
                .AsQueryable();

            foreach(var cond in rule.Conditions)
            {
                switch(cond.FieldType)
                {
                    case dto.ERecognitionRuleConditionFieldType.DataField:
                    {
                        var property = typeof(ImportedTransaction).GetProperty(cond.FieldName);

                        if(property == null)
                            throw new Exception($"Invalid ImportedTransaction property {cond.FieldName}");

                        switch(property.Name)
                        {
                            case "Amount":
                            {
                                var value = Convert.ToDecimal(cond.Value, CultureInfo.InvariantCulture);

                                switch(cond.ValueOperator)
                                {
                                    case ERecognitionRuleConditionOperator.Equals:
                                        transactions = transactions.Where(t => t.Amount == value);
                                        break;
                                    case ERecognitionRuleConditionOperator.Greater:
                                        transactions = transactions.Where(t => t.Amount > value);
                                        break;
                                    case ERecognitionRuleConditionOperator.GreaterOrEquals:
                                        transactions = transactions.Where(t => t.Amount >= value);
                                        break;
                                    case ERecognitionRuleConditionOperator.Lower:
                                        transactions = transactions.Where(t => t.Amount < value);
                                        break;
                                    case ERecognitionRuleConditionOperator.LowerOrEquals:
                                        transactions = transactions.Where(t => t.Amount <= value);
                                        break;
                                    default:
                                        new Exception($"Property {cond.FieldName} not supported for querying with operator {cond.ValueOperator}");
                                        break;
                                }
                            }
                                break;
                            case "ImportNumber":
                            {
                                switch(cond.ValueOperator)
                                {
                                    case ERecognitionRuleConditionOperator.Equals:
                                        transactions = transactions.Where(t => t.ImportNumber == cond.Value);
                                        break;
                                    case ERecognitionRuleConditionOperator.Contains:
                                        transactions = transactions.Where(t => t.ImportNumber.Contains(cond.Value));
                                        break;
                                    default:
                                        new Exception($"Property {cond.FieldName} not supported for querying with operator {cond.ValueOperator}");
                                        break;                                        
                                }
                            }
                                break;
                            case "ImportCaption":
                            {
                                switch(cond.ValueOperator)
                                {
                                    case ERecognitionRuleConditionOperator.Equals:
                                        transactions = transactions.Where(t => t.ImportCaption == cond.Value);
                                        break;
                                    case ERecognitionRuleConditionOperator.Contains:
                                        transactions = transactions.Where(t => t.ImportCaption.Contains(cond.Value));
                                        break;
                                    default:
                                        new Exception($"Property {cond.FieldName} not supported for querying with operator {cond.ValueOperator}");
                                        break;                                        
                                }
                            }
                                break;
                            case "ImportComment":
                            {
                                switch(cond.ValueOperator)
                                {
                                    case ERecognitionRuleConditionOperator.Equals:
                                        transactions = transactions.Where(t => t.ImportComment == cond.Value);
                                        break;
                                    case ERecognitionRuleConditionOperator.Contains:
                                        transactions = transactions.Where(t => t.ImportComment.Contains(cond.Value));
                                        break;
                                    default:
                                        new Exception($"Property {cond.FieldName} not supported for querying with operator {cond.ValueOperator}");
                                        break;                                        
                                }
                            }
                                break;
                            case "Caption":
                            {
                                switch(cond.ValueOperator)
                                {
                                    case ERecognitionRuleConditionOperator.Equals:
                                        transactions = transactions.Where(t => t.Caption == cond.Value);
                                        break;
                                    case ERecognitionRuleConditionOperator.Contains:
                                        transactions = transactions.Where(t => t.Caption.Contains(cond.Value));
                                        break;
                                    default:
                                        new Exception($"Property {cond.FieldName} not supported for querying with operator {cond.ValueOperator}");
                                        break;                                        
                                }
                            }
                                break;                            
                            case "Comment":
                            {
                                switch(cond.ValueOperator)
                                {
                                    case ERecognitionRuleConditionOperator.Equals:
                                        transactions = transactions.Where(t => t.Comment == cond.Value);
                                        break;
                                    case ERecognitionRuleConditionOperator.Contains:
                                        transactions = transactions.Where(t => t.Comment.Contains(cond.Value));
                                        break;
                                    default:
                                        new Exception($"Property {cond.FieldName} not supported for querying with operator {cond.ValueOperator}");
                                        break;                                        
                                }
                            }
                                break;
                            case "Date":
                            {
                                switch(cond.ValueOperator)
                                {
                                    case ERecognitionRuleConditionOperator.Equals:
                                        transactions = transactions.Where(t => t.Date.Date == DateTime.Parse(cond.Value, CultureInfo.InvariantCulture).Date);
                                        break;
                                    case ERecognitionRuleConditionOperator.DayEquals:
                                        transactions = transactions.Where(t => t.Date.Day == Convert.ToInt32(cond.Value));
                                        break;
                                    case ERecognitionRuleConditionOperator.MonthEquals:
                                        transactions = transactions.Where(t => t.Date.Month == Convert.ToInt32(cond.Value));
                                        break;
                                    case ERecognitionRuleConditionOperator.YearEquals:
                                        transactions = transactions.Where(t => t.Date.Year == Convert.ToInt32(cond.Value));
                                        break;
                                    case ERecognitionRuleConditionOperator.DayNear:
                                    {
                                        int value = Convert.ToInt32(cond.Value);
                                        int min = value - 3;
                                        int max = value + 3;

                                        // TODO : Si les min et max sont hors mois, il faudrait complexifier la requete pour gérer :
                                        // - les mois à 31 jours
                                        // - les mois à 30 jours
                                        // - les mois à 28 jours
                                        // Pour le moment, on simplifie en ne gérant pas ce cas de figure
                                        if(min < 1)
                                            min = 1;
                                        if(max > 31)
                                            min = 31;

                                        transactions = transactions.Where(t => t.Date.Day >= min && t.Date.Day <= max);
                                    }
                                        break;
                                    default:
                                        new Exception($"Property {cond.FieldName} not supported for querying with operator {cond.ValueOperator}");
                                        break;
                                }
                            }
                                break;
                            case "Type":
                            {
                                switch(cond.ValueOperator)
                                {
                                    case ERecognitionRuleConditionOperator.Equals:
                                        transactions = transactions.Where(t => t.Type == (ETransactionType)Convert.ToInt32(cond.Value));
                                        break;
                                    default:
                                        new Exception($"Property {cond.FieldName} not supported for querying with operator {cond.ValueOperator}");
                                        break;                                        
                                }
                            }
                                break;
                            case "UserDate":
                            {
                                switch(cond.ValueOperator)
                                {
                                    case ERecognitionRuleConditionOperator.Equals:
                                        transactions = transactions.Where(t => t.UserDate != null && t.UserDate.Value.Date == DateTime.Parse(cond.Value, CultureInfo.InvariantCulture).Date);
                                        break;
                                    case ERecognitionRuleConditionOperator.DayEquals:
                                        transactions = transactions.Where(t => t.UserDate != null && t.UserDate.Value.Day == Convert.ToInt32(cond.Value));
                                        break;
                                    case ERecognitionRuleConditionOperator.MonthEquals:
                                        transactions = transactions.Where(t => t.UserDate != null && t.UserDate.Value.Month == Convert.ToInt32(cond.Value));
                                        break;
                                    case ERecognitionRuleConditionOperator.YearEquals:
                                        transactions = transactions.Where(t => t.UserDate != null && t.UserDate.Value.Year == Convert.ToInt32(cond.Value));
                                        break;
                                    case ERecognitionRuleConditionOperator.DayNear:
                                    {
                                        int value = Convert.ToInt32(cond.Value);
                                        int min = value - 3;
                                        int max = value + 3;

                                        // TODO : Si les min et max sont hors mois, il faudrait complexifier la requete pour gérer :
                                        // - les mois à 31 jours
                                        // - les mois à 30 jours
                                        // - les mois à 28 jours
                                        // Pour le moment, on simplifie en ne gérant pas ce cas de figure
                                        if(min < 1)
                                            min = 1;
                                        if(max > 31)
                                            min = 31;

                                        transactions = transactions.Where(t => t.UserDate != null && t.UserDate.Value.Day >= min && t.UserDate.Value.Day <= max);
                                    }
                                        break;
                                    default:
                                        new Exception($"Property {cond.FieldName} not supported for querying with operator {cond.ValueOperator}");
                                        break;
                                }
                            }
                                break;
                            default:
                                throw new Exception($"Property {cond.FieldName} not supported for querying for now");
                        }

                        if(_logger.IsEnabled(LogLevel.Debug))
                        {
                            _logger.LogDebug($"Adding query property {cond.FieldName} {cond.ValueOperator} {cond.Value} : Remaining {transactions.Count()} transactions");
                        }
                    }
                        break;
                    case dto.ERecognitionRuleConditionFieldType.Tag:
                    {
                        var tag = await db.Tags.SingleOrDefaultAsync(t => t.TypeKey == cond.FieldName && t.Key == cond.Value);

                        if(tag == null)
                            throw new Exception($"Invalid tag {cond.FieldName}:{cond.Value}");

                        transactions = transactions.Where(t => t.TransactionTags.Any(tt => tt.TagId == tag.Id));

                        if(_logger.IsEnabled(LogLevel.Debug))
                        {
                            _logger.LogDebug($"Adding query tag {tag} : Remaining {transactions.Count()} transactions");
                        }
                    }
                        break;
                }
            }

            return transactions;
        }        
    }
}