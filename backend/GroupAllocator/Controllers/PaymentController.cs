using GroupAllocator.Database;
using GroupAllocator.Database.Model;
using GroupAllocator.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;
using System.IdentityModel.Tokens.Jwt;

namespace GroupAllocator.Controllers;

[ApiController]
[Route("[controller]")]
public class PaymentController(ApplicationDbContext db, PaymentService paymentService) : ControllerBase
{
	[HttpPut("{id}")]
	[Authorize(Policy = "TeacherOnly")]
	public async Task<IActionResult> UpgradeClass(int id)
	{
		if (paymentService.GetPaymentPlanForClass(id) == PaymentPlan.Basic)
		{
			return BadRequest("Already upgraded");
		}
		var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? throw new InvalidOperationException("No subject claim"));
		var user = await db.Teachers.FindAsync(userId) ?? throw new InvalidOperationException("User not found");
		var @class = await db.Classes.FindAsync(id) ?? throw new InvalidOperationException("Class not found"); ;

		var requiredCost = paymentService.CostOfPlan(PaymentPlan.Basic);
		// validate payment

		db.Payments.Add(new PaymentModel
		{
			Amount = requiredCost,
			ForClass = @class,
			PayedAt = DateTimeOffset.Now,
			Payer = user,
			Plan = PaymentPlan.Basic,
		});
		return Ok();
	}
}
