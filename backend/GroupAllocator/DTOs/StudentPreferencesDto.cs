namespace GroupAllocator.DTOs
{
	public class StudentPreferencesDto
	{
		public int[] Preferences { get; set; }
		public bool WillingToSignContract { get; set; }
		public string[] FileNames { get; set; }
		public byte[][] FileBlobs { get; set; }
	}
}
