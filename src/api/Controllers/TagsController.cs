using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text;
using Microsoft.EntityFrameworkCore;
using dal.Model;
using business.import;
using business;
using business.transaction.processor;
using business.extensions;
using Microsoft.Extensions.Logging;
using AutoMapper;
using dto.Model;

namespace api.Controllers
{
    [ApiController]
    [Route("tags")]
    [Produces("application/json")]
    public class TagsController : Controller
    {
        protected readonly dal.Model.MoneyboardContext _db;

        private readonly ILogger<TagsController> _logger;

        private readonly IMapper _mapper;

        public TagsController(dal.Model.MoneyboardContext db, ILogger<TagsController> logger, IMapper mapper)
        {
            _db = db;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet("")]
        public async Task<IActionResult> List()
        {
            return Json(_mapper.Map<IEnumerable<dto.Model.Tag>>(await _db.Tags.ToListAsync()));
        }

        [HttpGet("{tagTypeKey}")]
        public async Task<IActionResult> ListByTagType(string tagTypeKey)
        {
            return Json(_mapper.Map<IEnumerable<dto.Model.Tag>>(await _db.Tags.Where(t => t.TypeKey == tagTypeKey).ToListAsync()));
        }

        [HttpGet("{tagTypeKey}/{tagKeySource}")]
        public async Task<IActionResult> Details(string tagTypeKey, string tagKeySource)
        {
            var tag = await _db.Tags
                .Include(t => t.ParentTag)
                .Include(t => t.SubTags)
                .SingleOrDefaultAsync(t => t.TypeKey == tagTypeKey && t.Key == tagKeySource);

            if (tag == null)
                return NotFound();

            return Json(_mapper.Map<dto.Model.TagDetails>(tag));
        }

        [HttpPost("")]
        public async Task<IActionResult> Create(dto.Model.Tag tag)
        {
            _db.Tags.Add(_mapper.Map<dal.Model.Tag>(tag));

            await _db.SaveChangesAsync();

            return Json(tag);
        }

        [HttpPut("{tagTypeKey}/{tagKeySource}")]
        public async Task<IActionResult> Edit(string tagTypeKey, string tagKeySource, [FromBody] dto.Model.Tag tagData)
        {
            var dbTag = await _db.Tags.SingleOrDefaultAsync(t => t.TypeKey == tagTypeKey && t.Key == tagKeySource);

            if (dbTag == null)
                return NotFound();

            dbTag.Caption = tagData.Caption;

            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{tagTypeKey}/{tagKeySource}/statistics")]
        public async Task<IActionResult> Statistics(string tagTypeKey, string tagKeySource, [FromBody] TagStatisticsRequest request = null)
        {
            if (request == null)
                request = new TagStatisticsRequest();

            var tag = await _db.Tags
                .Include(t => t.SubTags)
                .SingleOrDefaultAsync(t => t.TypeKey == tagTypeKey && t.Key == tagKeySource);

            if (tag == null)
                return NotFound("Tag not found");

            var tags = new List<dal.Model.Tag>();
            tags.Add(tag);

            if (!request.IncludeSubTags)
            {
                var query = _db.Transactions
                    .Include(t => t.TransactionTags).ThenInclude(tt => tt.Tag)
                    .Where(t => t.TransactionTags.Any(tt => tags.Contains(tt.Tag)))
                    .AsQueryable();

                if (request.DateStart != null)
                    query = query.Where(t => t.Date >= request.DateStart);

                if (request.DateEnd != null)
                    query = query.Where(t => t.Date < request.DateEnd);

                if (request.AccountIds != null && request.AccountIds.Any())
                    query = query.Where(t => request.AccountIds.Contains(t.AccountId));

                switch (request.Range)
                {
                    case EDateRange.Days:
                        {
                            var result = query.GroupBy(t => new { Year = t.Date.Year, Month = t.Date.Month, Day = t.Date.Day }, (key, trx) => new TagStatisticsModel
                            {
                                Year = key.Year,
                                Month = key.Month,
                                Day = key.Day,
                                Total = trx.Sum(tx => tx.Amount)
                            });

                            return Json(result);
                        }
                    case EDateRange.Years:
                        {
                            var result = query.GroupBy(t => new { Year = t.Date.Year }, (key, trx) => new TagStatisticsModel
                            {
                                Year = key.Year,
                                Total = trx.Sum(tx => tx.Amount)
                            });

                            return Json(result);
                        }
                    case EDateRange.Months:
                    default:
                        {
                            var result = query.GroupBy(t => new { Year = t.Date.Year, Month = t.Date.Month }, (key, trx) => new TagStatisticsModel
                            {
                                Year = key.Year,
                                Month = key.Month,
                                Total = trx.Sum(tx => tx.Amount)
                            });

                            return Json(result);
                        }
                }
            }
            else
            {
                var dicAllSubTagIdsByTag = new Dictionary<dal.Model.Tag, IEnumerable<int>>();

                foreach(var subTag in tag.SubTags)
                {
                    tags.Add(subTag);

                    var subsubTags = await subTag.GetAllSubTagsAsync(_db);
                    tags.AddRange(subsubTags);

                    dicAllSubTagIdsByTag.Add(subTag, subsubTags.Select(st => st.Id).Append(subTag.Id));
                }

                var query = _db.Transactions
                    .Join(_db.TransactionTags.Include(tt => tt.Tag),
                        tx => tx.Id,
                        tt => tt.TransactionId,
                        (tx, tt) => new
                        {
                            Transaction = tx,
                            Tag = tt.Tag,
                        })
                    .Where(t => tags.Contains(t.Tag))
                    .AsQueryable();

                if (request.DateStart != null)
                    query = query.Where(t => t.Transaction.Date >= request.DateStart);

                if (request.DateEnd != null)
                    query = query.Where(t => t.Transaction.Date < request.DateEnd);

                if (request.AccountIds != null && request.AccountIds.Any())
                    query = query.Where(t => request.AccountIds.Contains(t.Transaction.AccountId));

                switch (request.Range)
                {
                    case EDateRange.Days:
                        {
                            var data = query.GroupBy(t => new { Year = t.Transaction.Date.Year, Month = t.Transaction.Date.Month, Day = t.Transaction.Date.Day, TagId = t.Tag.Id }, (key, trx) =>
                                new
                                {
                                    Year = key.Year,
                                    Month = key.Month,
                                    Day = key.Day,
                                    TagId = key.TagId,
                                    Amount = trx.Sum(t => t.Transaction.Amount)
                                }).ToList();

                            var result = new List<TagStatisticsModel>();

                            foreach(var period in data.Select(x => new { Year = x.Year, Month = x.Month, Day = x.Day }).OrderBy(x => x.Year).ThenBy(x => x.Month).ThenBy(x => x.Day).Distinct())
                            {
                                var periodData = data.Where(k => k.Year == period.Year && k.Month == period.Month && k.Day == period.Day);
                                var total = periodData.Sum(k => k.Amount);
                                var tagTotal = periodData.SingleOrDefault(k => k.TagId == tag.Id)?.Amount ?? 0m;

                                var subTagsTotal = new List<SingleTagStatistic>();

                                foreach(var subTag in tag.SubTags)
                                {
                                    var subSubTagIds = dicAllSubTagIdsByTag[subTag];

                                    subTagsTotal.Add(new SingleTagStatistic
                                    {
                                        Tag = _mapper.Map<dto.Model.Tag>(subTag),
                                        Amount = periodData.Where(d => subSubTagIds.Contains(d.TagId)).Sum(d => d.Amount)
                                    });
                                }

                                result.Add(new TagStatisticsModel
                                {
                                    Year = period.Year,
                                    Month = period.Month,
                                    Day = period.Day,
                                    Total = total,
                                    TagTotal = tagTotal,
                                    SubTagTotals = subTagsTotal
                                });
                            }

                            return Json(result);
                        }
                    case EDateRange.Years:
                        {
                            var data = query.GroupBy(t => new { Year = t.Transaction.Date.Year, TagId = t.Tag.Id }, (key, trx) =>
                                new
                                {
                                    Year = key.Year,
                                    TagId = key.TagId,
                                    Amount = trx.Sum(t => t.Transaction.Amount)
                                }).ToList();

                            var result = new List<TagStatisticsModel>();

                            foreach(var period in data.Select(x => new { Year = x.Year }).OrderBy(x => x.Year).Distinct())
                            {
                                var periodData = data.Where(k => k.Year == period.Year);
                                var total = periodData.Sum(k => k.Amount);
                                var tagTotal = periodData.SingleOrDefault(k => k.TagId == tag.Id)?.Amount ?? 0m;

                                var subTagsTotal = new List<SingleTagStatistic>();

                                foreach(var subTag in tag.SubTags)
                                {
                                    var subSubTagIds = dicAllSubTagIdsByTag[subTag];

                                    subTagsTotal.Add(new SingleTagStatistic
                                    {
                                        Tag = _mapper.Map<dto.Model.Tag>(subTag),
                                        Amount = periodData.Where(d => subSubTagIds.Contains(d.TagId)).Sum(d => d.Amount)
                                    });
                                }

                                result.Add(new TagStatisticsModel
                                {
                                    Year = period.Year,
                                    Total = total,
                                    TagTotal = tagTotal,
                                    SubTagTotals = subTagsTotal
                                });
                            }

                            return Json(result);
                        }
                    case EDateRange.Months:
                    default:
                        {
                            var data = query.GroupBy(t => new { Year = t.Transaction.Date.Year, Month = t.Transaction.Date.Month, TagId = t.Tag.Id }, (key, trx) =>
                                new
                                {
                                    Year = key.Year,
                                    Month = key.Month,
                                    TagId = key.TagId,
                                    Amount = trx.Sum(t => t.Transaction.Amount)
                                }).ToList();

                            var result = new List<TagStatisticsModel>();

                            foreach(var period in data.Select(x => new { Year = x.Year, Month = x.Month }).OrderBy(x => x.Year).ThenBy(x => x.Month).Distinct())
                            {
                                var periodData = data.Where(k => k.Year == period.Year && k.Month == period.Month);
                                var total = periodData.Sum(k => k.Amount);
                                var tagTotal = periodData.SingleOrDefault(k => k.TagId == tag.Id)?.Amount ?? 0m;

                                var subTagsTotal = new List<SingleTagStatistic>();

                                foreach(var subTag in tag.SubTags)
                                {
                                    var subSubTagIds = dicAllSubTagIdsByTag[subTag];

                                    subTagsTotal.Add(new SingleTagStatistic
                                    {
                                        Tag = _mapper.Map<dto.Model.Tag>(subTag),
                                        Amount = periodData.Where(d => subSubTagIds.Contains(d.TagId)).Sum(d => d.Amount)
                                    });
                                }

                                result.Add(new TagStatisticsModel
                                {
                                    Year = period.Year,
                                    Month = period.Month,
                                    Total = total,
                                    TagTotal = tagTotal,
                                    SubTagTotals = subTagsTotal
                                });
                            }

                            return Json(result);
                        }
                    }
                }
            }

            [HttpPost("{tagTypeKey}/{tagKeySource}/merge")]
            public async Task<IActionResult> MergeTags(string tagTypeKey, string tagKeySource, string target)
            {
                // 1 - Recherche les tags source et destination
                var tagSource = await _db.Tags.SingleOrDefaultAsync(t => t.TypeKey == tagTypeKey && t.Key == tagKeySource);

                if (tagSource == null)
                    return NotFound("Tag source not found");

                var tagTarget = await _db.Tags.SingleOrDefaultAsync(t => t.TypeKey == tagTypeKey && t.Key == target);

                if (tagTarget == null)
                    return NotFound("Tag target not found");

                // 2 - Rechercher les transactions de <tagKeySource>
                var transactions = _db.Transactions
                    .Include(t => t.TransactionTags).ThenInclude(tt => tt.Tag)
                    .Where(t => t.TransactionTags.Any(tt => tt.Tag == tagSource))
                    .ToList();

                // 3 - Modifier les tags vers <tagKeyTarget>
                foreach (var transaction in transactions)
                {
                    var transactionTag = transaction.TransactionTags.Single(tt => tt.Tag == tagSource);
                    transaction.TransactionTags.Remove(transactionTag);

                    transaction.TransactionTags.Add(new dal.Model.TransactionTag
                    {
                        Transaction = transaction,
                        Tag = tagTarget
                    });
                }

                // 4 - Créer une règle pour que les prochains imports de la clé <tagKeySource> soient taggués sur tagKeyTarget
                _db.TagRecognitions.Add(new TagRecognition
                {
                    RecognizedTagTypeKey = tagSource.TypeKey,
                    RecognizedTagKey = tagSource.Key,
                    TargetTag = tagTarget,
                });

                // 5 - Supprimer tagKeySource
                _db.Tags.Remove(tagSource);

                await _db.SaveChangesAsync();

                return Ok();
            }
        }
    }