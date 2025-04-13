namespace GroupAllocator.Database.Model
{
	public class FileModel
	{
		public int Id { get; set; }
		public required byte[] Blob { get; set; }
		public required string Name { get; set; }
		public required StudentModel Student { get; set; }
	}
}
