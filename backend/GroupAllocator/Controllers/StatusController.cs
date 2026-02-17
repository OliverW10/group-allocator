using System.Diagnostics;
using System.Text;
using GroupAllocator.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace GroupAllocator.Controllers;


[ApiController]
[Route("[controller]")]
public class StatusController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<string>> Status()
    {
        var response = new StringBuilder();
        response.AppendLine("Backend: GOOD (v1.0.1)");
        await AppendDbStatus(response);
        await AppendStripeStatus(response);
        return Ok(response.ToString());
    }

    async Task AppendDbStatus(StringBuilder response)
    {
        try
        {
            var sw = Stopwatch.StartNew();
            var canConnectToDb = await db.Database.CanConnectAsync();
            sw.Stop();
            response.AppendJoin(' ', "Database:", canConnectToDb ? $"GOOD ({sw.ElapsedMilliseconds}ms)" : "CANNOT CONNECT");
            if (!canConnectToDb)
            {
                response.AppendLine("\n\tMigrations: N/A");
            }
            else
            {
                response.Append("\n\tMigrations: ");
                var migrations = await db.Database.GetAppliedMigrationsAsync(CancellationToken.None);
                response.AppendJoin(", ", migrations);
                response.Append("\n");
            }
        }
        catch (Exception ex)
        {
            response.AppendLine("Exception while getting db status");
            response.AppendLine(ex.ToString());
        }
    }

    async Task AppendStripeStatus(StringBuilder response)
    {
        try
        {
            var sw = Stopwatch.StartNew();
            var account = await new BalanceService().GetAsync();
            sw.Stop();
            response.Append("Stripe Ping: GOOD (");
            response.Append(sw.ElapsedMilliseconds.ToString());
            response.AppendLine("ms)");
        }
        catch (Exception e)
        {
            response.Append("Stripe Ping: BAD (");
            response.Append(e.ToString());
            response.AppendLine(")");
        }
    }
}
