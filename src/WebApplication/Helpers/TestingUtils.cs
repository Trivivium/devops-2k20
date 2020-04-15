using System;

using Microsoft.EntityFrameworkCore;
using WebApplication.Entities;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace WebApplication.Helpers
{
    /*
     * Helper functions for the API spec tests
     * https://github.com/itu-devops/2020-spring/tree/master/sessions/session_02/API%20Spec
     */
    public class TestingUtils
    {
        private readonly ILogger<TestingUtils> _logger;
        private readonly DatabaseContext _database;

        public TestingUtils(ILogger<TestingUtils> logger, DatabaseContext database)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public async Task SetLatest(int val)
        {
            try 
            {
                var result = await _database.Latests.SingleOrDefaultAsync(l => l.id == 1);

                if (result == null)
                { 
                    _database.Latests.Add(new Latest
                    {
                       latest = val
                    });
                }
                else 
                {
                    result.latest = val;
                }
                
                _database.SaveChanges();
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("Operation canceled while updating latest value.");
            }
        }
        
        public static async Task<Latest> GetLatest(DatabaseContext databaseContext)
        {
            var result = await databaseContext.Latests.SingleOrDefaultAsync(l => l.id == 1);

            return result;
        }
    }
}
