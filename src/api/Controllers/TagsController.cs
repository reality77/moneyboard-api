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
    [Route("tags")]
    [Produces("application/json")]
    public class TagsController : Controller
    {
        protected readonly dal.Model.MoneyboardContext _db;

        private readonly ILogger<TagsController> _logger;

        public TagsController(dal.Model.MoneyboardContext db, ILogger<TagsController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet("")]
        public IActionResult List()
        {
            //TODO : DTO
            return Json(_db.Tags.Include(t => t.ParentTag).Select(t => new {
                TypeKey = t.TypeKey,
                Key = t.Key,
                Caption = t.Caption,
                ParentTypeKey = (t.ParentTag != null) ? t.ParentTag.TypeKey : null,
                ParentKey = (t.ParentTag != null) ? t.ParentTag.Key : null,
            }));
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