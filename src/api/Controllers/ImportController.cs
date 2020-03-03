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

        public ImportController(dal.Model.MoneyboardContext db, ILogger<ImportController> logger, IServiceProvider serviceProvider)
        {
            _db = db;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        [HttpGet("")]
        public IActionResult List()
        {
            return Json(_db.ImportedFiles.Include(f => f.Transactions));
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
    }
}