using GroupAllocator.DTOs;

namespace GroupAllocator.Database.Model
{
	public class FileModel
	{
		public int Id { get; set; }
		public required byte[] Blob { get; set; }
		public required string Name { get; set; }
		public required UserModel User { get; set; }
	}

	public static class FileModelExtensions
	{
		public static FileDetailsDto ToDto(this FileModel fileModel)
		{
			return new FileDetailsDto()
			{
				Id = fileModel.Id,
				Name = fileModel.Name,
				UserId = fileModel.User.Id
			};
		}
	}
}
