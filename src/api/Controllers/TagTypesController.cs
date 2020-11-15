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
    [Route("tagtypes")]
    [Produces("application/json")]
    public class TagTypesController : Controller
    {
        protected readonly dal.Model.MoneyboardContext _db;

        private readonly ILogger<TagTypesController> _logger;

        private readonly IMapper _mapper;

        public TagTypesController(dal.Model.MoneyboardContext db, ILogger<TagTypesController> logger, IMapper mapper)
        {
            _db = db;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet("")]
        public async Task<IActionResult> List()
        {
            return Json(_mapper.Map<IEnumerable<dto.Model.TagType>>(await _db.TagTypes.ToListAsync()));
        }

        [HttpGet("{tagTypeKey}")]
        public async Task<IActionResult> Details(string tagTypeKey)
        {
            var result = await _db.TagTypes.SingleOrDefaultAsync(t => t.Key == tagTypeKey);

            if(result == null)
                return NotFound();
            else
                return Json(_mapper.Map<dto.Model.TagType>(result));
        }
    }
}