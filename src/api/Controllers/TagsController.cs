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
using api.Models;

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
            var tag = await _db.Tags.SingleOrDefaultAsync(t => t.TypeKey == tagTypeKey && t.Key == tagKeySource);

            if(tag == null)
                return NotFound();

            return Json(_mapper.Map<dto.Model.Tag>(tag));
        }

        [HttpPost("")]
        public async Task<IActionResult> Create(dto.Model.Tag tag)
        {
            _db.Tags.Add(_mapper.Map<dal.Model.Tag>(tag));
            
            await _db.SaveChangesAsync();

            return Json(tag);
        }

        [HttpPost("{tagTypeKey}/{tagKeySource}/statistics")]
        public async Task<IActionResult> Statistics(string tagTypeKey, string tagKeySource, [FromBody] TagStatisticsRequest request = null)
        {
            if(request == null)
                request = new TagStatisticsRequest();

            var tag = await _db.Tags.SingleOrDefaultAsync(t => t.TypeKey == tagTypeKey && t.Key == tagKeySource);

            if(tag == null)
                return NotFound("Tag not found");

            var query = _db.Transactions
                .Include(t => t.TransactionTags).ThenInclude(tt => tt.Tag)
                .Where(t => t.TransactionTags.Any(tt => tt.Tag == tag))
                .AsQueryable();

            if(request.IncludeSubTags)
                throw new NotImplementedException("TODO : request.IncludeSubTags");

            if(request.DateStart != null)
                query = query.Where(t => t.Date >= request.DateStart);

            if(request.DateEnd != null)
                query = query.Where(t => t.Date < request.DateEnd);

            if(request.AccountIds != null && request.AccountIds.Any())
                query = query.Where(t => request.AccountIds.Contains(t.AccountId));

            switch(request.Range)
            {
                case EDateRange.Days:
                {
                    var result = query.GroupBy(t => new { Year = t.Date.Year, Month = t.Date.Month, Day = t.Date.Day }, (key, trx) => new 
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
                    var result = query.GroupBy(t => new { Year = t.Date.Year }, (key, trx) => new 
                    {
                        Year = key.Year,
                        Total = trx.Sum(tx => tx.Amount)
                    });

                    return Json(result);
                }
                case EDateRange.Months:
                default:
                {
                    var result = query.GroupBy(t => new { Year = t.Date.Year, Month = t.Date.Month }, (key, trx) => new 
                    {
                        Year = key.Year,
                        Month = key.Month,
                        Total = trx.Sum(tx => tx.Amount)
                    });

                    return Json(result);
                }            
            }
        }

        [HttpPost("{tagTypeKey}/{tagKeySource}/merge")]
        public async Task<IActionResult> MergeTags(string tagTypeKey, string tagKeySource, string target)
        {
            // 1 - Recherche les tags source et destination
            var tagSource = await _db.Tags.SingleOrDefaultAsync(t => t.TypeKey == tagTypeKey && t.Key == tagKeySource);

            if(tagSource == null)
                return NotFound("Tag source not found");

            var tagTarget = await _db.Tags.SingleOrDefaultAsync(t => t.TypeKey == tagTypeKey && t.Key == target);

            if(tagTarget == null)
                return NotFound("Tag target not found");

            // 2 - Rechercher les transactions de <tagKeySource>
            var transactions = _db.Transactions
                .Include(t => t.TransactionTags).ThenInclude(tt => tt.Tag)
                .Where(t => t.TransactionTags.Any(tt => tt.Tag == tagSource))
                .ToList();

            // 3 - Modifier les tags vers <tagKeyTarget>
            foreach(var transaction in transactions)
            {
                var transactionTag = transaction.TransactionTags.Single(tt => tt.Tag == tagSource);
                transaction.TransactionTags.Remove(transactionTag);

                transaction.TransactionTags.Add(new TransactionTag
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