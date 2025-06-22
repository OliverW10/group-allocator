using GroupAllocator.Database;
using GroupAllocator.Database.Model;
using GroupAllocator.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Stripe.Checkout;
using System.IdentityModel.Tokens.Jwt;

namespace GroupAllocator.Controllers;

[ApiController]
[Route("[controller]")]
public class PaymentController(ApplicationDbContext db, PaymentService paymentService) : ControllerBase
{
	[HttpPut("{id}")]
	[Authorize(Policy = "TeacherOnly")]
	public async Task<ActionResult> UpgradeClass(int id)
	{
		var @class = await db.Classes.Include(c => c.Payments).FirstAsync(c => c.Id == id) ?? throw new InvalidOperationException("Class not found");
		if (paymentService.GetPaymentPlanForClass(@class) == PaymentPlan.Basic)
		{
			return BadRequest("Already upgraded");
		}
		var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? throw new InvalidOperationException("No subject claim"));
		var user = await db.Users.FindAsync(userId) ?? throw new InvalidOperationException("User not found");

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

	[HttpGet("create")]
	[Authorize(Policy = "TeachersOnly")]
	public async Task<ActionResult<object>> CreateStripeSession(string returnDomain)
	{
		var options = new SessionCreateOptions
		{
			UiMode = "embedded",
			LineItems = new List<SessionLineItemOptions>
				{
				  new SessionLineItemOptions
				  {
                    // Provide the exact Price ID (for example, price_1234) of the product you want to sell
                    Price = "price_1RaCBJGfx18ZU63nZwODuoLS",
					Quantity = 1,
				  },
				},
			Mode = "payment",
			ReturnUrl = returnDomain + "?session_id={CHECKOUT_SESSION_ID}",
		};
		var service = new SessionService();
		Session session = await service.CreateAsync(options);

		return new { clientSecret = session.ClientSecret };
	}
}
