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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using business.import.processor;
using AutoMapper;

namespace api.Controllers
{
    [ApiController]
    [Route("import")]
    [Produces("application/json")]
    public class ImportController : Controller
    {
        protected readonly dal.Model.MoneyboardContext _db;
        private readonly ILogger<ImportController> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMapper _mapper;

        public ImportController(dal.Model.MoneyboardContext db, ILogger<ImportController> logger, IServiceProvider serviceProvider, IMapper mapper)
        {
            _db = db;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _mapper = mapper;
        }

        [HttpGet("")]
        public IActionResult List()
        {
            return Json(_mapper.Map<IEnumerable<dto.Model.ImportedFile>>(_db.ImportedFiles));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var file = await _db.ImportedFiles.SingleOrDefaultAsync(f => f.Id == id);

            if(file == null)
                return NotFound();
            
            return Json(_mapper.Map<dto.Model.ImportedFile>(file));
        }
        
        [HttpGet("byfilename")]
        public async Task<IActionResult> Details(string name)
        {
            var file = await _db.ImportedFiles.SingleOrDefaultAsync(f => f.FileName.ToLower() == name.ToLower());

            if(file == null)
                return NotFound();
            
            return Json(_mapper.Map<dto.Model.ImportedFile>(file));
        }

        [HttpPost("")]
        public async Task<IActionResult> Import(int accountId)
        {
            var account = _db.Accounts.SingleOrDefault(a => a.Id == accountId);

            if(account == null)
                return NotFound();

            var dicErrors = new Dictionary<string, TransactionsFileImportResult>();
            
            foreach (var file in Request.Form.Files)
            {
                ImportedFile ifile = new ImportedFile
                {
                    FileName = file.Name,
                    ImportDate = DateTime.Now,
                };

                var stream = file.OpenReadStream();

                // --- Import des données du fichier
                ImporterBase importer = null;

                if (file.FileName.ToLower().EndsWith(".ofx"))
                {
                    importer = new OFXImporter();
                }
                else if (file.FileName.ToLower().EndsWith(".qif"))
                {
                    importer = new QIFImporter();
                }
                else
                {
                    return BadRequest("[IMPORT] File extension not supported");
                }

                // Ajout processeurs
                importer.Processors.Add(new DatabaseInsertionProcessor(_db, account, new List<ITransactionProcessor>()
                {
                    new CaisseEpargneProcessor(),
                    new RecognitionRulesProcessor(_serviceProvider.GetService<ILogger<RecognitionRulesProcessor>>()),
                }, _serviceProvider.GetService<ILogger<DatabaseInsertionProcessor>>()));

                try
                {
                    var importResult = importer.Import(file.FileName, stream);
                    dicErrors.Add(file.FileName, importResult);

                    await _db.SaveChangesAsync();

                }
                catch (Exception ex)
                {
                    return BadRequest($"[IMPORT] {ex.ToString()}");
                }
            }

            return Json(dicErrors); // POUR DEBUG
        }

        [HttpPost("{id}/rescan")]
        public async Task<IActionResult> RescanFile(int id)
        {
            var transactions = _db.ImportedTransactions.Where(t => t.ImportFileId == id);

            var dicErrors = new Dictionary<string, TransactionsFileImportResult>();
            var transactionProcessors = new List<ITransactionProcessor>()
                {
                    new CaisseEpargneProcessor(),
                    new RecognitionRulesProcessor(_serviceProvider.GetService<ILogger<RecognitionRulesProcessor>>())
                };

            foreach (var transaction in transactions)
            {
                // Suppression tags
                transaction.TransactionTags.Clear();
                await _db.SaveChangesAsync();

                try
                {
                    foreach(var processor in transactionProcessors)
                    {
                        _logger.LogDebug($"Processing {processor.GetType().Name} for transaction {transaction.ImportHash}");
                        processor.ProcessTransaction(_db, transaction);
                        _db.SaveChanges();
                    }

                    await _db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    return BadRequest($"[IMPORT] {ex.ToString()}");
                }
            }

            return Json(dicErrors); // POUR DEBUG
        }        
    }
}