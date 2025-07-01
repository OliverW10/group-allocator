namespace GroupAllocator.Database.Model;

public enum PaymentPlan
{
	None, Basic
}

public class PaymentModel
{
	public int Id { get; set; }
	public required UserModel Payer { get; set; }
	public required ClassModel ForClass { get; set; }
	public required decimal Amount { get; set; }
	public required DateTimeOffset PayedAt { get; set; }
	public required PaymentPlan Plan { get; set; }
	public required string StripeSessionId { get; set; }
	public required string Currency { get; set; }
}
