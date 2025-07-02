using GroupAllocator.Database;
using GroupAllocator.Database.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.IdentityModel.Tokens.Jwt;

namespace GroupAllocator.Controllers;

[ApiController]
[Route("[controller]")]
public class PaymentController(ApplicationDbContext db) : ControllerBase
{
	[HttpGet("create-stripe-session")]
	[Authorize(Policy = "TeacherOnly")]
	public async Task<ActionResult<string>> CreateStripeSession(string returnDomain)
	{
		var options = new SessionCreateOptions
		{
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
			SuccessUrl = returnDomain + "?success=true&session_id={CHECKOUT_SESSION_ID}",
			CancelUrl = returnDomain + "?success=false&session_id={CHECKOUT_SESSION_ID}",
		};
		var service = new SessionService();
		Session session = await service.CreateAsync(options);

		return session.Url;
	}

	[HttpGet("verify-payment")]
	[Authorize(Policy = "TeacherOnly")]
	public async Task<ActionResult<bool>> VerifyPayment(string sessionId, int classId)
	{
		var @class = await db.Classes.FindAsync(classId) ?? throw new InvalidOperationException("Class not found");
		var user = await db.Users.FindAsync(int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? throw new InvalidOperationException("No subject claim"))) ?? throw new InvalidOperationException("User not found");
		if (string.IsNullOrEmpty(sessionId))
		{
			return BadRequest("Session ID is required");
		}

		try
		{
			var service = new SessionService();
			var session = await service.GetAsync(sessionId);

			if (session == null)
			{
				return false;
			}
			if (session.PaymentStatus != "paid" || session.AmountTotal is null)
			{
				return false;
			}

			var paymentModel = new PaymentModel {
				Amount = (decimal)session.AmountTotal,
				ForClass = @class,
				PayedAt = new DateTimeOffset(session.Created),
				Payer = user,
				Plan = PaymentPlan.Basic,
				StripeSessionId = session.Id,
				Currency = session.Currency,
			};

			db.Payments.Add(paymentModel);
			await db.SaveChangesAsync();

			return Ok(true);
		}
		catch (StripeException ex)
		{
			return BadRequest(new { error = ex.Message });
		}
	}

	
}
