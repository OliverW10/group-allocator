namespace GroupAllocator.Database.Model;

public enum PaymentPlan
{
	None, Basic
}

public class PaymentModel
{
	public required UserModel Payer { get; set; }
	public required ClassModel ForClass { get; set; }
	public required decimal Amount { get; set; }
	public required DateTimeOffset PayedAt { get; set; }
	public required PaymentPlan Plan { get; set; }
	// probably need a tracking number of something from stripe
}
