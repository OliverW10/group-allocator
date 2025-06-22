using GroupAllocator.Database.Model;

namespace GroupAllocator.Services;

public class PaymentService
{
	public PaymentPlan GetPaymentPlanForClass(ClassModel @class)
	{
		// Store and perform checks on plan instead of price to allow changing price without invalidating existing classes
		return @class.Payments.Any(p => p.Plan == PaymentPlan.Basic) ? PaymentPlan.Basic : PaymentPlan.None;
	}

	public decimal CostOfPlan(PaymentPlan plan)
	{
		return plan switch
		{
			PaymentPlan.None => 0,
			PaymentPlan.Basic => 5,
			_ => throw new InvalidOperationException("Not a valid enum"),
		};
	}
}
