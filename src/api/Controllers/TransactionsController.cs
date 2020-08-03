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

namespace api.Controllers
{
    [ApiController]
    [Route("transactions")]
    [Produces("application/json")]
    public class TransactionsController : Controller
    {
        protected readonly dal.Model.MoneyboardContext _db;

        private readonly ILogger<TransactionsController> _logger;

        private readonly IMapper _mapper;

        public TransactionsController(dal.Model.MoneyboardContext db, ILogger<TransactionsController> logger, IMapper mapper)
        {
            _db = db;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet("")]
        public IActionResult List()
        {
            var trx = _db.Transactions.Include(t => t.TransactionTags).ThenInclude(tt => tt.Tag);

            return Json(_mapper.Map<IEnumerable<dto.Model.Transaction>>(trx));
        }


        [HttpGet("imported")]
        public IActionResult ListImported()
        {
            var trx = _db.ImportedTransactions.Include(t => t.TransactionTags).ThenInclude(tt => tt.Tag);

            return Json(_mapper.Map<IEnumerable<dto.Model.ImportedTransaction>>(trx));
        }


        [HttpGet("{id}")]
        public IActionResult Details(int id)
        {
            var trx = _db.ImportedTransactions
                .Include(t => t.TransactionTags)
                .ThenInclude(tt => tt.Tag)
                .SingleOrDefault(t => t.Id == id);

            if(trx == null)
                return NotFound();

            return Json(_mapper.Map<dto.Model.ImportedTransaction>(trx));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] dto.Model.TransactionEditRequest transaction)
        {
            var trx = _db.Transactions
                .SingleOrDefault(t => t.Id == id);

            if(trx == null)
                return NotFound();

            trx.Caption = transaction.Caption;
            trx.Comment = transaction.Comment;
            trx.Date = transaction.Date.Date;
            if(transaction.UserDate != null)
                trx.UserDate = transaction.UserDate.Value.Date;
            else
                trx.UserDate = null;

            await _db.SaveChangesAsync();

            return NoContent();
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
            return Json(_mapper.Map<IEnumerable<dto.Model.Transaction>>(query));
        }
    }
}