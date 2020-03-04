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

namespace api.Controllers
{
    [ApiController]
    [Route("transactions")]
    [Produces("application/json")]
    public class TransactionsController : Controller
    {
        protected readonly dal.Model.MoneyboardContext _db;

        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(dal.Model.MoneyboardContext db, ILogger<TransactionsController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet("")]
        public IActionResult List()
        {
            return Json(_db.ImportedTransactions.Include(t => t.TransactionTags).ThenInclude(tt => tt.Tag));
        }

        [HttpGet("tag/{tagTypeKey}/{tagKey}")]
        public async Task<IActionResult> ByTag(string tagTypeKey, string tagKey, bool searchSubTags = false)
        {
            var tag = await _db.Tags.SingleOrDefaultAsync(tg => tg.TypeKey == tagTypeKey && tg.Key == tagKey);

            if(tag == null)
                return NotFound($"Tag {tagTypeKey}.{tagKey} not found");

            var query = _db.Transactions
                .Include(t => t.TransactionTags).ThenInclude(tt => tt.Tag).AsQueryable();

            if(searchSubTags) 
            {
                var tagIds = (await tag.GetAllSubTagsAsync(_db)).Select(tg => tg.Id);
                query = query.Where(t => t.TransactionTags.Any(tt => tagIds.Contains(tt.Tag.Id)));
            }
            else
            {
                query = query.Where(t => t.TransactionTags.Any(tt => tt.Tag == tag));
            }

            //TODO DTO
            return Json(query.Select(t => new 
            {
                Id = t.Id,
                Date = t.Date,
                Caption = t.Caption,
                Amount = t.Amount,
                Comment = t.Comment,
                Tags = t.TransactionTags.Select(tt => new 
                {
                    TypeKey = tt.Tag.TypeKey,
                    Key = tt.Tag.Key,
                })
            }));
        }
    }
}