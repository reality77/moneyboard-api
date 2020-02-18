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

namespace api.Controllers
{
    [ApiController]
    [Route("transactions")]
    [Produces("application/json")]
    public class TransactionsController : Controller
    {
        protected readonly dal.Model.MoneyboardContext _db;

        public TransactionsController(dal.Model.MoneyboardContext db)
        {
            _db = db;
        }

        [HttpGet("")]
        public IActionResult List()
        {
            return Json(_db.ImportedTransactions.Include(t => t.TransactionTags).ThenInclude(tt => tt.Tag));
        }
    }
}