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

namespace api.Controllers
{
    [ApiController]
    [Route("import")]
    [Produces("application/json")]
    public class ImportController : Controller
    {
        protected readonly dal.Model.MoneyboardContext _db;

        public ImportController(dal.Model.MoneyboardContext db)
        {
            _db = db;
        }

        [HttpPost("")]
        public IActionResult Import()
        {
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

                try
                {
                    var importedFile = importer.Import(stream, out var importErrors);
                    return Json(importedFile); // POUR DEBUG
                }
                catch (Exception ex)
                {
                    return BadRequest($"[IMPORT] {ex.ToString()}");
                }
            }

            return Ok();
        }
    }
}