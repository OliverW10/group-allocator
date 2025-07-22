namespace GroupAllocator.Database.Model;

public class ClientLimitModel {
    public int Id { get; set; }
    public required ClientModel Client { get; set; }
    public required int MinProjects { get; set; }
    public required int MaxProjects { get; set; }
    public required SolveRunModel SolveRun { get; set; }
}
