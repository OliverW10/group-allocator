using GroupAllocator.Database.Model;
using GroupAllocator.Services;

List<StudentModel> students = [
    new StudentModel() {
        WillSignContract = true,
        User = new UserModel() {
            Email = "alice@uts.com",
            Name = "Alice Surname",
            IsAdmin = false,
        }
    },
    new StudentModel() {
        WillSignContract = false,
        User = new UserModel() {
            Email = "bob@uts.com",
            Name = "Bob Lastname",
            IsAdmin = false,
        }
    },
    new StudentModel() {
        WillSignContract = true,
        User = new UserModel() {
            Email = "charlie@uts.com",
            Name = "Charlie Familyname",
            IsAdmin = false,
        }
    },
];

List<ClientModel> clients = [
    new ClientModel() {
        MaxProjects = 2,
        MinProjects = 1,
        Name = "Client pty ltd"
    }
];

List<ProjectModel> projects = [
    new ProjectModel() {
        Client = clients[0],
        Name = "AI Slop",
        RequiresNda = false,
        MinStudents = 1,
        MaxStudents = 2
    },
    new ProjectModel() {
        Client = clients[0],
        Name = "Crypto Scam",
        RequiresNda = true,
        MinStudents = 3,
        MaxStudents = 4
    }
];

List<PreferenceModel> preferences = [
    new PreferenceModel() {
        Id = 1,
        Student = students[0],
        Project = projects[0],
        Strength = 1.0
    },
    new PreferenceModel() {
        Id = 2,
        Student = students[0],
        Project = projects[1],
        Strength = 0.7
    },
    new PreferenceModel() {
        Id = 3,
        Student = students[1],
        Project = projects[0],
        Strength = 0.7
    }
];

var solver = new AllocationSolver();
solver.AssignStudentsToGroups(students, projects, clients, preferences);