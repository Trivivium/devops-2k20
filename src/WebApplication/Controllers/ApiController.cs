using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using WebApplication.Entities;
using WebApplication.Helpers;

namespace WebApplication.Controllers
{
    [Authorize]
    [Route("api")]
    public class ApiController : ControllerBase
    {
        private readonly DatabaseContext _databaseContext;

        public ApiController(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
        }

        [HttpGet("/latest")]
        public async Task<IActionResult> GetLatest(CancellationToken ct)
        {
            var latest = await TestingUtils.GetLatest(_databaseContext, ct);
            
            return Ok(latest);
        }
        
        [HttpGet("/msgs")]
        public async Task<IActionResult> GetMessages(CancellationToken ct)
        {
            throw new NotImplementedException();
        }
        
        [HttpGet("/msgs/:username")]
        public async Task<IActionResult> GetMessagesFromUser(CancellationToken ct)
        {
            throw new NotImplementedException();
        }
        
        [HttpPost("/msgs/:username")]
        public async Task<IActionResult> AddMessageToUser(CancellationToken ct)
        {
            throw new NotImplementedException();
        }
        
        [HttpGet("/fllws/:username")]
        public async Task<IActionResult> GetFollowersFromUser(CancellationToken ct)
        {
            throw new NotImplementedException();
        }
        
        [HttpPost("/fllws/:username")]
        public async Task<IActionResult> AddOrRemoveFollowerFromUser(CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
