namespace GroupAllocator.Database.Model;

public class PaymentModel
{
	public required TeacherModel Payer { get; set; }
	public required ClassModel ForClass { get; set; }
	public required decimal Amount { get; set; }
	public required DateTimeOffset PayedAt { get; set; }
	// probably need a tracking number of something from stripe
}
