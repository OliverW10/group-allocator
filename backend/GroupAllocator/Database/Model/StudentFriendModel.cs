

namespace GroupAllocator.Database.Model;

public class StudentFriendModel
{
    public int Id { get; set; }
    public required StudentModel Student { get; set; }
    public required StudentModel Friend { get; set; }

    public ICollection<StudentFriendModel> Friends { get; } = new List<StudentFriendModel>();
}