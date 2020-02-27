using Microsoft.EntityFrameworkCore;
using WebApplication.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace WebApplication.Helpers
{
    /*
     * Helper functions for the API spec tests
     * https://github.com/itu-devops/2020-spring/tree/master/sessions/session_02/API%20Spec
     */
    public  static class TestingUtils
    {

        public static async Task SetLatest(DatabaseContext databaseContext, int val, CancellationToken ct)
        {
            var result = await databaseContext.Latests.SingleOrDefaultAsync(l => l.id == 1, ct);

            if (result == null)
            { 
                databaseContext.Latests.Add(new Latest
                {
                   latest = val
                });
            }
            else 
            {
                result.latest = val;
            }
            
            databaseContext.SaveChanges();
        }
        public static async Task<Latest> GetLatest(DatabaseContext databaseContext, CancellationToken ct)
        {
            var result = await databaseContext.Latests.SingleOrDefaultAsync(l => l.id == 1, ct);

            return result;
        }
    }
}
