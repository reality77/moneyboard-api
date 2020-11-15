using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text;
using Microsoft.EntityFrameworkCore;
using business.import;
using business;
using business.transaction.processor;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using business.import.processor;
using AutoMapper;
using System.Net;
using AutoMapper.QueryableExtensions;

namespace api.Controllers
{
    [ApiController]
    [Route("recognition")]
    [Produces("application/json")]
    public class RecognitionController : Controller
    {
        protected readonly dal.Model.MoneyboardContext _db;
        private readonly ILogger<RecognitionController> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMapper _mapper;

        public RecognitionController(dal.Model.MoneyboardContext db, ILogger<RecognitionController> logger, IServiceProvider serviceProvider, IMapper mapper)
        {
            _db = db;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _mapper = mapper;
        }


        [HttpGet("rules")]
        public IActionResult RuleList()
        {
            var rules = _db.TransactionRecognitionRules
                .Include(r => r.Conditions)
                .Include(r => r.Actions)
                .AsQueryable()
                .ProjectTo<dto.Model.TransactionRecognitionRule>(_mapper.ConfigurationProvider);
            return Json(rules);
        }

        [HttpGet("rules/{id}")]
        public async Task<IActionResult> RuleGet(int id)
        {
            var rule = await _db.TransactionRecognitionRules
                .Include(r => r.Conditions)
                .Include(r => r.Actions)            
                .SingleOrDefaultAsync(r => r.Id == id);

            if(rule == null)
                return NotFound();

            return Json(_mapper.Map<dto.Model.TransactionRecognitionRule>(rule));
        }

        [HttpPost("rules")]
        public async Task<IActionResult> RuleCreate(dto.Model.TransactionRecognitionRuleEdit rule)
        {
            var dbrule = _mapper.Map<dal.Model.TransactionRecognitionRule>(rule);

            await _db.TransactionRecognitionRules.AddAsync(dbrule);

            await _db.SaveChangesAsync();

            return Json(_mapper.Map<dto.Model.TransactionRecognitionRule>(dbrule));
        }


        [HttpDelete("rules/{id}")]
        public async Task<IActionResult> RuleDelete(int id)
        {
            var dbrule = await _db.TransactionRecognitionRules.SingleOrDefaultAsync(r => r.Id == id);

            _db.TransactionRecognitionRules.Remove(dbrule);

            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("rescan/transaction/{id}")]
        public async Task<IActionResult> RescanTransaction(int id)
        {
            var transactions = _db.ImportedTransactions
                .Include(t => t.TransactionTags)
                .ThenInclude(tt => tt.Tag)
                .ThenInclude(t => t.Type)                
                .Where(t => t.Id == id);

            try
            {
                var result = await RescanInternal(transactions);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"RESCAN TRANSACTION {id} : {ex.ToString()}");
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("rescan/importedfile/{id}")]
        public async Task<IActionResult> RescanFile(int id)
        {
            var transactions = _db.ImportedTransactions
                .Include(t => t.TransactionTags)
                .ThenInclude(tt => tt.Tag)
                .ThenInclude(t => t.Type)                
                .Where(t => t.ImportFileId == id);

            try
            {
                var result = await RescanInternal(transactions);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"RESCAN FILE {id} : {ex.ToString()}");
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("rescan/all")]
        public async Task<IActionResult> RescanAll()
        {
            var transactions = _db.ImportedTransactions
                .Include(t => t.TransactionTags)
                .ThenInclude(tt => tt.Tag)
                .ThenInclude(t => t.Type)
                .AsQueryable();

            try
            {
                var result = await RescanInternal(transactions);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"RESCAN ALL : {ex.ToString()}");
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        private async Task<Dictionary<string, TransactionsFileImportResult>> RescanInternal(IQueryable<dal.Model.ImportedTransaction> transactions)
        {
            var dicErrors = new Dictionary<string, TransactionsFileImportResult>();

            var transactionProcessors = new List<ITransactionProcessor>()
                {
                    new CaisseEpargneProcessor(_serviceProvider.GetService<ILogger<CaisseEpargneProcessor>>()),
                    new RecognitionRulesProcessor(_serviceProvider.GetService<ILogger<RecognitionRulesProcessor>>())
                };

            foreach (var transaction in await transactions.ToListAsync())
            {
                // Suppression des tags générés automatiquement
                foreach(var tt in transaction.TransactionTags.ToList())
                {
                    if(!tt.IsManual)
                        transaction.TransactionTags.Remove(tt);
                }
                
                await _db.SaveChangesAsync();

                foreach(var processor in transactionProcessors)
                {
                    _logger.LogDebug($"Processing {processor.GetType().Name} for transaction {transaction.ImportHash}");
                    processor.ProcessTransaction(_db, transaction);
                    _db.SaveChanges();
                }

                await _db.SaveChangesAsync();
            }

            return dicErrors; // POUR DEBUG
        }
    }
}