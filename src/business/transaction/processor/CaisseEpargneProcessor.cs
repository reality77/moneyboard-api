using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using dal.Model;
using Microsoft.EntityFrameworkCore;
using dto;
using Microsoft.Extensions.Logging;

namespace business.transaction.processor
{
    public class CaisseEpargneProcessor : ITransactionProcessor
    {
        private readonly ILogger<CaisseEpargneProcessor> _logger;
        const string USERDATE_FORMAT = "user_date_";
        const string TAG_FORMAT = "tag_";

        private static Dictionary<Regex, ETransactionType> s_regexes = new Dictionary<Regex, ETransactionType>();

        static CaisseEpargneProcessor()
        {
            s_regexes.Add(new Regex("^VIR (?'tag_mode'(.*?)) (?'tag_payee'(.*))$", RegexOptions.Compiled), ETransactionType.Transfer);
            s_regexes.Add(new Regex("^CB (?'tag_payee'(.*)) (?'comment'(.*?)) (?'user_date_FR'(.*))$", RegexOptions.Compiled), ETransactionType.Payment);
            s_regexes.Add(new Regex("^RETRAIT (?'tag_mode'(.*?)) (?'user_date_FR'(.*?)) (?'comment'(.*))$", RegexOptions.Compiled), ETransactionType.Withdrawal);
            s_regexes.Add(new Regex("^PRLV (?'tag_payee'(.*))$", RegexOptions.Compiled), ETransactionType.Debit);
            s_regexes.Add(new Regex("^* (?'caption'(.*))$", RegexOptions.Compiled), ETransactionType.Fees);
        }

        public CaisseEpargneProcessor(ILogger<CaisseEpargneProcessor> logger) => _logger = logger;

        public void ProcessTransaction(MoneyboardContext db, ImportedTransaction transaction)
        {
            _logger.LogDebug($"Processing transaction {transaction.Id}");
            
            foreach(var regexItem in s_regexes)
            {
                if(string.IsNullOrWhiteSpace(transaction.ImportCaption))
                {
                    _logger.LogInformation($"No caption detected in transaction {transaction.Id}");
                    continue;
                }

                var match = regexItem.Key.Match(transaction.ImportCaption);

                if (match.Success)
                {
                    transaction.Type = regexItem.Value;

                    if(transaction.Type == ETransactionType.Withdrawal && string.IsNullOrEmpty(transaction.Caption))
                        transaction.Caption = "Retrait";

                    transaction.UserDate = DetectUserDate(match);

                    var tags = DetectTags(db, match);


                    foreach(var tag in tags)
                    {
                        bool addTag = true;

                        if(tag.Type.OneTagOnly)
                        {
                            // recherche d'autres tags du meme type
                            foreach(var ttag in transaction.TransactionTags.Where(tt => tt.Tag.Type == tag.Type).ToList())
                            {
                                if(ttag.IsManual)
                                {
                                    addTag = false;
                                    _logger.LogInformation($"Manual tag {ttag.Tag} detected in transaction {transaction.Id} : The tag {tag} will not be added (cause : OneTagOnly)");
                                    break;
                                }

                                if(ttag.Tag.Key != tag.Key)
                                {
                                    _logger.LogInformation($"Removing tag {ttag.Tag} from transaction {transaction.Id} (cause : OneTagOnly)");
                                    transaction.TransactionTags.Remove(ttag);
                                }
                                else
                                {
                                    _logger.LogInformation($"Keeping existing tag {ttag.Tag} in transaction {transaction.Id} (cause : OneTagOnly)");
                                    addTag = false;
                                }
                            }
                        }

                        if(addTag)
                        {
                            _logger.LogInformation($"Adding tag {tag} to transaction {transaction.Id}");

                            transaction.TransactionTags.Add(new TransactionTag
                            {
                                Transaction = transaction,
                                Tag = tag,
                            });
                        }
                    }

                    transaction.Caption = DetectGroup("caption", match);
                    transaction.Comment = DetectGroup("comment", match);

                    break;
                }    
            }
        }

        private IEnumerable<Tag> DetectTags(MoneyboardContext db, Match match)
        {
            List<Tag> tags = new List<Tag>();

            var groups = (IEnumerable<Group>)match.Groups;
            groups = groups.Where(g => g.Name.StartsWith(TAG_FORMAT));

            foreach (var group in groups)
            {
                var tagTypeKey = group.Name.Remove(0, TAG_FORMAT.Length);
                var tagKey = FormatTagKey(group.Value.Trim());
                var tagKeyCaption = group.Value.Trim();

                var recognizedTag = db.TagRecognitions
                    .Include(tr => tr.TargetTag)
                    .SingleOrDefault(tr => tr.RecognizedTagTypeKey == tagTypeKey && tr.RecognizedTagKey == tagKey);

                Tag tag;

                if(recognizedTag != null)
                    tag = recognizedTag.TargetTag;
                else
                {
                    var tagType = db.TagTypes
                        .Include(t => t.Tags)
                        .SingleOrDefault(t => t.Key == tagTypeKey);

                    if (tagType == null)
                    {
                        // Pour le moment, on créé les types de tags à la volée. A voir si on garde à l'avenir
                        tagType = new TagType
                        {
                            Key = tagTypeKey,
                            Caption = tagTypeKey,
                        };

                        db.TagTypes.Add(tagType);
                    }

                    tag = db.Tags.SingleOrDefault(t => t.TypeKey == tagTypeKey && t.Key == tagKey);

                    if (tag == null)
                    {
                        tag = new Tag
                        {
                            Type = tagType,
                            TypeKey = tagTypeKey,
                            Key = tagKey,
                            Caption = tagKeyCaption,
                        };

                        db.Tags.Add(tag);
                    }
                }

                tags.Add(tag);
            }

            return tags;
        }

        private string FormatTagKey(string tag)
        {
            return tag.ToLower().Replace(' ', '_').Replace("/", "__");
        }

        private string DetectGroup(string groupName, Match match)
        {
            var groups = (IEnumerable<Group>)match.Groups;
            var group = groups.SingleOrDefault(g => g.Name == groupName);

            if(group != null)
                return group.Value;

            return null;
        }

        private DateTime? DetectUserDate(Match match)
        {
            var groups = (IEnumerable<Group>)match.Groups;
            var group = groups.SingleOrDefault(g => g.Name.StartsWith(USERDATE_FORMAT));
            
            if(group != null)
            {
                var userDateCulture = group.Name.Remove(0, USERDATE_FORMAT.Length);
                
                List<string> dateFormats = new List<string>();
                switch(userDateCulture)
                {
                    case "FR":
                    {
                        dateFormats.Add("ddMMyyyy");
                        dateFormats.Add("ddMMyy");
                    }
                        break;
                    case "EN":
                    {
                        dateFormats.Add("MMddyyyy");
                        dateFormats.Add("MMddyy");
                    }
                        break;
                }

                if(DateTime.TryParseExact(group.Value, dateFormats.ToArray(), DateTimeFormatInfo.CurrentInfo, DateTimeStyles.None, out DateTime date))
                    return date;
                else
                {
                    //TODO : errors
                    return null;
                }
            }

            return null;
        }
    }
}