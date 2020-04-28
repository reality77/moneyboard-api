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
using dto.Model;

namespace api.Controllers
{
    [ApiController]
    [Route("accounts")]
    [Produces("application/json")]
    public class AccountsController : Controller
    {
        protected readonly dal.Model.MoneyboardContext _db;

        private readonly ILogger<AccountsController> _logger;

        private readonly IMapper _mapper;

        public AccountsController(dal.Model.MoneyboardContext db, ILogger<AccountsController> logger, IMapper mapper)
        {
            _db = db;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet("")]
        public IActionResult List()
        {
            var accounts = _db.Accounts.AsQueryable();

            return Json(_mapper.Map<IEnumerable<AccountBase>>(accounts));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var account = await _db.Accounts.SingleOrDefaultAsync(a => a.Id == id);

            if(account == null)
                return NotFound();

            return Json(_mapper.Map<AccountDetails>(account));
        }

        [HttpPost("")]
        public async Task<IActionResult> Create(AccountDetails account)
        {
            if(_db.Accounts.Any(a => a.Name == account.Name || (account.Number != null && a.Number == account.Number) || (account.Iban != null && a.Iban == account.Iban)))
                return BadRequest(); // todo préciser erreur (doublon)

            var dbacc = new dal.Model.Account
            {
                Name = account.Name,
                Number = account.Number,
                InitialBalance = account.InitialBalance,
                Currency = account.Currency,
                Iban = account.Iban,
            };

            _db.Accounts.Add(dbacc);

            await _db.SaveChangesAsync();

            return Created($"/accounts/{dbacc.Id}", _mapper.Map<AccountDetails>(dbacc));
        }

        [HttpGet("by")]
        public async Task<IActionResult> Details(string number)
        {
            var account = await _db.Accounts.SingleOrDefaultAsync(a => a.Number == number);

            if(account == null)
                return NotFound();

            return Json(_mapper.Map<AccountDetails>(account));
        }

        [HttpGet("{id}/transactions")]
        public async Task<IActionResult> Transactions(int id, int pageId = 0, int itemsPerPage = 500)
        {
            var transactions = await _db.Transactions
                .Include(t => t.BalanceData)
                .Include(t => t.TransactionTags).ThenInclude(tt => tt.Tag)
                .Where(t => t.AccountId == id)
                .OrderByDescending(t => t.Date)
                .Skip(pageId * itemsPerPage)
                .Take(itemsPerPage)
                .ToListAsync();

            return Json(_mapper.Map<IEnumerable<TransactionWithBalance>>(transactions));
        }

        [HttpGet("{id}/balance_history")]
        public IActionResult BalanceHistory(int id, DateTime? from, DateTime? to)
        {
            var transactions = _db.Transactions
                .Include(t => t.BalanceData)
                .Include(t => t.TransactionTags).ThenInclude(tt => tt.Tag)
                .Where(t => t.AccountId == id)
                .OrderBy(t => t.Date).AsQueryable();

            if(from != null)
                transactions.Where(t => t.Date >= from);

            if(to != null)
                transactions.Where(t => t.Date < to);

            var result = transactions.Select(t => new AccountBalance
                { 
                    Date = t.Date, 
                    Balance = t.BalanceData.Balance 
                });

            return Json(result);
        }
    }
}