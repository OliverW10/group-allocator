using GroupAllocator.Database.Model;
using GroupAllocator.Services;

//baseline test

//List<StudentModel> students = [
//    new StudentModel { Id = 1, WillSignContract = true,  User = new UserModel { Name = "Alice", Email = "alice@uts.com", IsAdmin = false } },
//    new StudentModel { Id = 2, WillSignContract = false, User = new UserModel { Name = "Bob", Email = "bob@uts.com", IsAdmin = false } },
//    new StudentModel { Id = 3, WillSignContract = true,  User = new UserModel { Name = "Charlie", Email = "charlie@uts.com", IsAdmin = false } },
//    new StudentModel { Id = 4, WillSignContract = true,  User = new UserModel { Name = "Diana", Email = "diana@uts.com", IsAdmin = false } },
//    new StudentModel { Id = 5, WillSignContract = false, User = new UserModel { Name = "Ethan", Email = "ethan@uts.com", IsAdmin = false } },
//    new StudentModel { Id = 6, WillSignContract = true,  User = new UserModel { Name = "Fay", Email = "fay@uts.com", IsAdmin = false } },
//    new StudentModel { Id = 7, WillSignContract = true,  User = new UserModel { Name = "Grace", Email = "grace@uts.com", IsAdmin = false } },
//    new StudentModel { Id = 8, WillSignContract = true,  User = new UserModel { Name = "Henry", Email = "henry@uts.com", IsAdmin = false } },
//];

//List<ClientModel> clients = [
//    new ClientModel { Id = 1, Name = "Tech Co", MinProjects = 1, MaxProjects = 2 },
//    new ClientModel { Id = 2, Name = "Health Org", MinProjects = 1, MaxProjects = 1 }
//];

//List<ProjectModel> projects = [
//    new ProjectModel { Id = 1, Name = "AI Tutor",         RequiresNda = false, Client = clients[0], MinStudents = 3, MaxStudents = 4 },
//    new ProjectModel { Id = 2, Name = "Health Tracker",   RequiresNda = true,  Client = clients[1], MinStudents = 3, MaxStudents = 4},
//    new ProjectModel { Id = 3, Name = "Blockchain Ledger",RequiresNda = false, Client = clients[0], MinStudents = 3, MaxStudents = 4}
//];

//List<PreferenceModel> preferences = [
//    new PreferenceModel { Id = 1, Student = students[0], Project = projects[0], Strength = 1.0 },
//    new PreferenceModel { Id = 2, Student = students[1], Project = projects[0], Strength = 0.8 },
//    new PreferenceModel { Id = 3, Student = students[2], Project = projects[0], Strength = 0.9 },
//    new PreferenceModel { Id = 4, Student = students[3], Project = projects[1], Strength = 1.0 },
//    new PreferenceModel { Id = 5, Student = students[4], Project = projects[1], Strength = 0.7 }, 
//    new PreferenceModel { Id = 6, Student = students[5], Project = projects[2], Strength = 0.85 },
//    new PreferenceModel { Id = 7, Student = students[6], Project = projects[2], Strength = 0.95 },
//    new PreferenceModel { Id = 8, Student = students[7], Project = projects[2], Strength = 0.75 },
//    new PreferenceModel { Id = 9, Student = students[0], Project = projects[1], Strength = 0.6 },
//    new PreferenceModel { Id = 10, Student = students[3], Project = projects[2], Strength = 0.8 },
//    new PreferenceModel { Id = 11, Student = students[1], Project = projects[2], Strength = 0.4 },
//    new PreferenceModel { Id = 12, Student = students[2], Project = projects[1], Strength = 0.9 }
//];

//testing group constraint
List<StudentModel> students = [
    new StudentModel { Id = 1, WillSignContract = true, User = new UserModel { Email = "a@uts.com", Name = "Alice", IsAdmin = false } },
    new StudentModel { Id = 2, WillSignContract = true, User = new UserModel { Email = "b@uts.com", Name = "Bob", IsAdmin = false } },
    new StudentModel { Id = 3, WillSignContract = false, User = new UserModel { Email = "c@uts.com", Name = "Charlie", IsAdmin = false } },
    new StudentModel { Id = 4, WillSignContract = true, User = new UserModel { Email = "d@uts.com", Name = "Diana", IsAdmin = false } },
    new StudentModel { Id = 5, WillSignContract = true, User = new UserModel { Email = "e@uts.com", Name = "Ethan", IsAdmin = false } },
    new StudentModel { Id = 6, WillSignContract = true, User = new UserModel { Email = "f@uts.com", Name = "Fay", IsAdmin = false } },
    new StudentModel { Id = 7, WillSignContract = true, User = new UserModel { Email = "g@uts.com", Name = "Grace", IsAdmin = false } },
    new StudentModel { Id = 8, WillSignContract = true, User = new UserModel { Email = "h@uts.com", Name = "Henry", IsAdmin = false } },
    new StudentModel { Id = 9, WillSignContract = false, User = new UserModel { Email = "i@uts.com", Name = "Ivy", IsAdmin = false } },
    new StudentModel { Id = 10, WillSignContract = true, User = new UserModel { Email = "j@uts.com", Name = "Jack", IsAdmin = false } }
];

List<ClientModel> clients = [
    new ClientModel { Id = 1, Name = "Tech Corp", MinProjects = 1, MaxProjects = 2 },
    new ClientModel { Id = 2, Name = "Health Ltd", MinProjects = 1, MaxProjects = 2 },
    new ClientModel { Id = 3, Name = "Edu Labs", MinProjects = 0, MaxProjects = 1 }
];

List<ProjectModel> projects = [

    new ProjectModel { Id = 1, Name = "AI Tutor", RequiresContract = false, RequiresNda = false, Client = clients[0], MinStudents = 3, MaxStudents = 4 },
    new ProjectModel { Id = 2, Name = "Fitness App", RequiresContract = false, RequiresNda = false, Client = clients[1], MinStudents = 3, MaxStudents = 4 },
    new ProjectModel { Id = 3, Name = "Crypto Wallet", RequiresContract = true, RequiresNda = true, Client = clients[0], MinStudents = 3, MaxStudents = 4 },
    new ProjectModel { Id = 4, Name = "Online Exam Tool", RequiresContract = false, RequiresNda = false, Client = clients[2], MinStudents = 2, MaxStudents = 3 },
    new ProjectModel { Id = 5, Name = "Nutrition Tracker", RequiresContract = false, RequiresNda = false, Client = clients[1], MinStudents = 2, MaxStudents = 3 }
];

List<PreferenceModel> preferences = [

    new PreferenceModel { Id = 1, Student = students[0], Project = projects[0], Strength = 1.0 },
    new PreferenceModel { Id = 2, Student = students[1], Project = projects[0], Strength = 0.8 },
    new PreferenceModel { Id = 3, Student = students[2], Project = projects[1], Strength = 0.9 },
    new PreferenceModel { Id = 4, Student = students[3], Project = projects[1], Strength = 1.0 },
    new PreferenceModel { Id = 5, Student = students[4], Project = projects[0], Strength = 0.6 },
    new PreferenceModel { Id = 6, Student = students[5], Project = projects[1], Strength = 0.7 },
    new PreferenceModel { Id = 7, Student = students[6], Project = projects[2], Strength = 0.9 },
    new PreferenceModel { Id = 8, Student = students[7], Project = projects[2], Strength = 1.0 },
    new PreferenceModel { Id = 9, Student = students[8], Project = projects[3], Strength = 0.6 },
    new PreferenceModel { Id = 10, Student = students[9], Project = projects[4], Strength = 0.8 },
    new PreferenceModel { Id = 11, Student = students[3], Project = projects[3], Strength = 0.5 },
    new PreferenceModel { Id = 12, Student = students[1], Project = projects[4], Strength = 0.7 },
    new PreferenceModel { Id = 13, Student = students[6], Project = projects[0], Strength = 0.6 },
    new PreferenceModel { Id = 14, Student = students[9], Project = projects[2], Strength = 0.9 },
];


var solver = new AllocationSolver();
var assignments = solver.AssignStudentsToGroups(students, projects, clients, preferences);


if (!assignments.Any())
{
    Console.WriteLine("No assignments");
}

else
{
    foreach (var assignment in assignments)
    {
        Console.WriteLine($"{assignment.Student.User.Name} assigned to {assignment.Project.Name}");
    }
}
