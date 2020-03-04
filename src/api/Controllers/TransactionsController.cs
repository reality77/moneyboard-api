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
            var trx = _db.ImportedTransactions.Include(t => t.TransactionTags).ThenInclude(tt => tt.Tag);

            return Json(_mapper.Map<IEnumerable<dto.Model.Transaction>>(trx));
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