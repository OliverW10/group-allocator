using GroupAllocator.Database;
using GroupAllocator.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace GroupAllocator.Services;

public class PaymentService(ApplicationDbContext db)
{
	public PaymentPlan GetPaymentPlanForClass(int classId)
	{
		// Store and perform checks on plan instead of price to allow changing price without invalidating existing classes
		return db.Payments.Include(p => p.ForClass).Where(p => p.ForClass.Id == classId).Select(p => p.Plan).Aggregate(PaymentPlan.None, AddPlans);

		PaymentPlan AddPlans(PaymentPlan first, PaymentPlan second)
		{
			if (first == PaymentPlan.Basic || second == PaymentPlan.Basic) return PaymentPlan.Basic;
			return PaymentPlan.None;
		}
	}

	public decimal CostOfPlan(PaymentPlan plan)
	{
		return plan switch
		{
			PaymentPlan.None => 0,
			PaymentPlan.Basic => 5,
		};
	}
}
