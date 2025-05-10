using GroupAllocator.Database.Model;
using GroupAllocator.DTOs;
using GroupAllocator.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.FileSystemGlobbing;
using static Google.Protobuf.WellKnownTypes.Field.Types;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

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


////testing group constraint
//List<StudentModel> students = [
//    new StudentModel { Id = 1, WillSignContract = true, User = new UserModel { Email = "a@uts.com", Name = "Alice", IsAdmin = false } },
//    new StudentModel { Id = 2, WillSignContract = true, User = new UserModel { Email = "b@uts.com", Name = "Bob", IsAdmin = false } },
//    new StudentModel { Id = 3, WillSignContract = false, User = new UserModel { Email = "c@uts.com", Name = "Charlie", IsAdmin = false } },
//    new StudentModel { Id = 4, WillSignContract = true, User = new UserModel { Email = "d@uts.com", Name = "Diana", IsAdmin = false } },
//    new StudentModel { Id = 5, WillSignContract = true, User = new UserModel { Email = "e@uts.com", Name = "Ethan", IsAdmin = false } },
//    new StudentModel { Id = 6, WillSignContract = true, User = new UserModel { Email = "f@uts.com", Name = "Fay", IsAdmin = false } },
//    new StudentModel { Id = 7, WillSignContract = true, User = new UserModel { Email = "g@uts.com", Name = "Grace", IsAdmin = false } },
//    new StudentModel { Id = 8, WillSignContract = true, User = new UserModel { Email = "h@uts.com", Name = "Henry", IsAdmin = false } },
//    new StudentModel { Id = 9, WillSignContract = false, User = new UserModel { Email = "i@uts.com", Name = "Ivy", IsAdmin = false } },
//    new StudentModel { Id = 10, WillSignContract = true, User = new UserModel { Email = "j@uts.com", Name = "Jack", IsAdmin = false } }
//];

//List<ClientModel> clients = [
//    new ClientModel { Id = 1, Name = "Tech Corp", MinProjects = 1, MaxProjects = 2 },
//    new ClientModel { Id = 2, Name = "Health Ltd", MinProjects = 1, MaxProjects = 2 },
//    new ClientModel { Id = 3, Name = "Edu Labs", MinProjects = 0, MaxProjects = 1 }
//];

//List<ProjectModel> projects = [

//    new ProjectModel { Id = 1, Name = "AI Tutor", RequiresContract = false, RequiresNda = false, Client = clients[0], MinStudents = 3, MaxStudents = 4 },
//    new ProjectModel { Id = 2, Name = "Fitness App", RequiresContract = false, RequiresNda = false, Client = clients[1], MinStudents = 3, MaxStudents = 4 },
//    new ProjectModel { Id = 3, Name = "Crypto Wallet", RequiresContract = true, RequiresNda = true, Client = clients[0], MinStudents = 3, MaxStudents = 4 },
//    new ProjectModel { Id = 4, Name = "Online Exam Tool", RequiresContract = false, RequiresNda = false, Client = clients[2], MinStudents = 2, MaxStudents = 3 },
//    new ProjectModel { Id = 5, Name = "Nutrition Tracker", RequiresContract = false, RequiresNda = false, Client = clients[1], MinStudents = 2, MaxStudents = 3 }
//];

//List<PreferenceModel> preferences = [

//    new PreferenceModel { Id = 1, Student = students[0], Project = projects[0], Strength = 1.0 },
//    new PreferenceModel { Id = 2, Student = students[1], Project = projects[0], Strength = 0.8 },
//    new PreferenceModel { Id = 3, Student = students[2], Project = projects[1], Strength = 0.9 },
//    new PreferenceModel { Id = 4, Student = students[3], Project = projects[1], Strength = 1.0 },
//    new PreferenceModel { Id = 5, Student = students[4], Project = projects[0], Strength = 0.6 },
//    new PreferenceModel { Id = 6, Student = students[5], Project = projects[1], Strength = 0.7 },
//    new PreferenceModel { Id = 7, Student = students[6], Project = projects[2], Strength = 0.9 },
//    new PreferenceModel { Id = 8, Student = students[7], Project = projects[2], Strength = 1.0 },
//    new PreferenceModel { Id = 9, Student = students[8], Project = projects[3], Strength = 0.6 },
//    new PreferenceModel { Id = 10, Student = students[9], Project = projects[4], Strength = 0.8 },
//    new PreferenceModel { Id = 11, Student = students[3], Project = projects[3], Strength = 0.5 },
//    new PreferenceModel { Id = 12, Student = students[1], Project = projects[4], Strength = 0.7 },
//    new PreferenceModel { Id = 13, Student = students[6], Project = projects[0], Strength = 0.6 },
//    new PreferenceModel { Id = 14, Student = students[9], Project = projects[2], Strength = 0.9 },
//];



//Real data test case

List<ClientModel> clients = [

    new ClientModel { Id = 1, Name = "Chris Howell" },
    new ClientModel { Id = 2, Name = "Henry Gilder" },
    new ClientModel { Id = 3, Name = "Lindsay Lyon" },
    new ClientModel { Id = 4, Name = "Mitra Gusheh" },
    new ClientModel { Id = 5, Name = "Peter McArdle" },
    new ClientModel { Id = 6, Name = "Gavin Kocsis" },
    new ClientModel { Id = 7, Name = "Louis Fernandez" },
    new ClientModel { Id = 8, Name = "Mick Kane" },
    new ClientModel { Id = 9, Name = "Ahmed Rafiq" },
    new ClientModel { Id = 10, Name = "Sheila Sutjipto" },
    new ClientModel { Id = 11, Name = "Marc Carmichael" },
    new ClientModel { Id = 12, Name = "Isira Wijegunawardana" },
    new ClientModel { Id = 13, Name = "Wade Marynowsky" },
    new ClientModel { Id = 14, Name = "Katrina Leung" },
    new ClientModel { Id = 15, Name = "Rafael Luna Zelaya" },
    new ClientModel { Id = 16, Name = "Steven Vasilescu" },
    new ClientModel { Id = 17, Name = "Rodney Berry" },
    new ClientModel { Id = 18, Name = "David Chambers" },

];

List<ProjectModel> projects = [

    new ProjectModel { Id = 101, Name = "StevTech Docking Station", RequiresNda = true, Client = clients[0], MinStudents = 4, MaxStudents = 5 },
    new ProjectModel { Id = 102, Name = "StevTech Computer Vision", RequiresNda = true, Client = clients[0], MinStudents = 4, MaxStudents = 5 },
    new ProjectModel { Id = 103, Name = "SPYDER", RequiresNda = true, Client = clients[1], MinStudents = 4, MaxStudents = 5 },
    new ProjectModel { Id = 104, Name = "GIAGO Placement", RequiresNda = true, Client = clients[2], MinStudents = 4, MaxStudents = 5 },
    new ProjectModel { Id = 105, Name = "GIAGO Feeder", RequiresNda = true, Client = clients[2], MinStudents = 4, MaxStudents = 5 },
    new ProjectModel { Id = 106, Name = "Glebe Youth Services - Bash the Trash", RequiresNda = false, Client = clients[3], MinStudents = 4, MaxStudents = 4 },
    new ProjectModel { Id = 108, Name = "EWB", RequiresNda = false, Client = clients[4], MinStudents = 4, MaxStudents = 4 },
    new ProjectModel { Id = 109, Name = "Pura Link", RequiresNda = true, Client = clients[5], MinStudents = 4, MaxStudents = 4 },
    new ProjectModel { Id = 110, Name = "SupaStretch @ Home Model", RequiresNda = false, Client = clients[7], MinStudents = 4, MaxStudents = 4 },
    new ProjectModel { Id = 203, Name = "Ball dropping robot", RequiresNda = false, Client = clients[11], MinStudents = 4, MaxStudents = 4 },
    new ProjectModel { Id = 204, Name = "Robot in the gallery", RequiresNda = false, Client = clients[12], MinStudents = 4, MaxStudents = 4 },
    new ProjectModel { Id = 205, Name = "3D printing filament maker", RequiresNda = false, Client = clients[13], MinStudents = 4, MaxStudents = 4 },
    new ProjectModel { Id = 207, Name = "Spatial Micro-climates", RequiresNda = false, Client = clients[14], MinStudents = 4, MaxStudents = 4 },
    new ProjectModel { Id = 208, Name = "Energy Wall", RequiresNda = false, Client = clients[14], MinStudents = 4, MaxStudents = 4 },
    new ProjectModel { Id = 209, Name = "Heated jig for white tip forming", RequiresNda = true, Client = clients[6], MinStudents = 4, MaxStudents = 4 },
    new ProjectModel { Id = 210, Name = "Futsal goal solution", RequiresNda = false, Client = clients[10], MinStudents = 4, MaxStudents = 4 },
    new ProjectModel { Id = 301, Name = "FEIT Rocketry: static fire test stand", RequiresNda = false, Client = clients[10], MinStudents = 4, MaxStudents = 4 },
    new ProjectModel { Id = 302, Name = "FEIT Rocketry: filament winder", RequiresNda = false, Client = clients[10], MinStudents = 4, MaxStudents = 4 },
    new ProjectModel { Id = 303, Name = "FEIT Rover: Field Operations Optimization", RequiresNda = false, Client = clients[10], MinStudents = 4, MaxStudents = 5 },
    new ProjectModel { Id = 304, Name = "FEIT MSA: Autonomous Steeying Mechanism", RequiresNda = false, Client = clients[17], MinStudents = 4, MaxStudents = 4 },
    new ProjectModel { Id = 305, Name = "FEIT MSE : Design and Manufacture of a Composite Seat", RequiresNda = false, Client = clients[17], MinStudents = 4, MaxStudents = 4 },
    new ProjectModel { Id = 306, Name = "FEIT MSE : Ready to move light", RequiresNda = false, Client = clients[17], MinStudents = 4, MaxStudents = 4 },
    new ProjectModel { Id = 307, Name = "FEIT MSE : Diffuser", RequiresNda = false, Client = clients[17], MinStudents = 4, MaxStudents = 4 }
];


//list of student IDs
List<int> studentIDs = new List<int>() {13888060, 14289680
,14288017
,14271302
,14289692
,24548238
,25638331
,13569951
,14282661
,24556526
,14283511
,14299491
,14254953
,14173177
,13893870
,25031675
,13898299
,14255121
,14047563
,13938752
,25079450
,13936048
,13908658
,14305116
,13581172
,14174472
,12895672
,13342997
,12948434
,14075814
,13930437
,24577355
,14243024
,25707441
,13764939
,24553620

,13572464
,14257339
,13565270
,14250157
,13889276
,13940352
,25126917
,13626991
,14328114
,13893639
,13900081

,14093254
,24656769
,14282406
,25083585
,13200908
,13926757
,13938802
,24473555
,24450759
,14265262
,14367825
,14185843
,98123781
,14267924
,13043827
,13898616};


//names of students
List<string> names = new List<string>() {"Alison Akhurst",
"Alexander Vergara"
,"William Thomas"
,"Philopateer Abdelmalek"
,"Matthew Egan"
,"Alexander Dickson"
,"Hew Wee Ng"
,"Alan Kinal"
,"Daniel Bell"
,"Dean Tsafnat"
,"James Harris"
,"Morgan Sheather-Reid"
,"Tomas Mihalic"
,"Lucas Moore"
,"Kavieth Munasinghe"
,"Romil Kharia"
,"Geoffrey Cosmo"
,"George Attie"
,"Imran Orr"
,"Jacob Ryan"
,"Dinh Nguyen"
,"Daniel Nguyen"
,"Daniel Zucak"
,"James Haitidis"
,"Thien Tran"
,"Paige Iwatani"
,"Shahd Sumrain"
,"Li Feilong"
,"Evie Raffin"
,"Serey Mongkul Te"
,"Madhav Diwan"
,"Marcin Konczyk"
,"Timothy Kruik"
,"Timothy Linkert"
,"Sam Yip"
,"Nathan Turner"

,"Jesse Brown"
,"Dana Apoderado"
,"Sophie Allum"
,"Jaemon Lamey"
,"Nicola Katsianos"
,"Sophie Anderson"
,"Cooper Perry"
,"Steven Nguyen"
,"Lonish Padarath"
,"Christian Mihic"
,"Vince Pirina"

,"Sheba Thomas"
,"Brendan White"
,"Daniel Chen"
,"Eshya Liyanagamage"
,"Toon Janssen"
,"Morne Kruger"
,"Ryan Thomas"
,"Mackenzie Thompson"
,"Declan Chamma"
,"Dylan Selge"
,"Sanka Kotinkaduwa"
,"Abhinash Tiwary"
,"Stewart Kelly"
,"Maximilian Deda"
,"Maximilian Ioannidis"
,"Connor Higgins" };


//project id's ranked from best to worst for students
//note that excel file ranks all the projects not ten but i just did ten
List<List<int>> individualPreferences = new List<List<int>>
{
    new List<int> { 303, 101, 102, 104, 105, 106, 108, 103, 203, 110 },
    new List<int> { 303, 207, 301, 209, 101, 102, 306, 305, 205, 203 },
    new List<int> { 210, 106, 103, 101, 108, 110, 207, 307, 102, 104 },
    new List<int> { 307, 301, 210, 101, 108, 207, 305, 203, 106, 302 },
    new List<int> { 205, 103, 302, 101, 307, 208, 207, 306, 303, 301 },

    new List<int> { 303, 103, 205, 105, 101, 110, 102, 301, 209, 104 },
    new List<int> { 106, 110, 101, 104, 208, 105, 109, 108, 102, 103 },
    new List<int> { 210, 301, 305, 307, 306, 108, 110, 106, 203, 207 },
    new List<int> { 101, 103, 207, 208, 203, 110, 209, 301, 205, 204 },
    new List<int> { 301, 302, 307, 305, 306, 304, 303, 101, 102, 103 },

    new List<int> { 303, 208, 110, 210, 103, 101, 102, 301, 307, 104 },
    new List<int> { 109, 102, 101, 103, 104, 105, 106, 108, 110, 203 },
    new List<int> { 101, 103, 205, 210, 108, 203, 204, 104, 207, 208 },
    new List<int> { 301, 302, 103, 209, 203, 101, 102, 104, 109, 306 },
    new List<int> { 101, 103, 108, 105, 106, 104, 209, 207, 203, 204 },
    new List<int> { 207, 307, 208, 305, 306, 301, 302, 304, 209, 303 },
    new List<int> { 205, 209, 210, 101, 301, 108, 103, 203, 306, 106 },
    new List<int> { 205, 101, 103, 304, 301, 307, 303, 203, 207, 208 },
    new List<int> { 101, 103, 209, 105, 104, 303, 203, 302, 205, 204 },
    new List<int> { 101, 102, 103, 104, 105, 109, 108, 110, 209, 203 },

    new List<int> { 209, 110, 102, 304, 103, 204, 101, 306, 307, 106 },
    new List<int> { 204, 205, 203, 306, 104, 101, 106, 302, 210, 109 },
    new List<int> { 101, 110, 204, 108, 301, 205, 302, 210, 304, 207 },
    new List<int> { 307, 305, 304, 210, 110, 101, 103, 306, 207, 204 },
    new List<int> { 109, 101, 102, 103, 104, 105, 106, 108, 110, 203 },
    new List<int> { 205, 103, 302, 101, 207, 307, 208, 303, 306, 301 },
    new List<int> { 102, 106, 208, 210, 203, 205, 110, 209, 204, 108 },
    new List<int> { 306, 301, 108, 204, 210, 307, 203, 304, 109, 209 },
    new List<int> { 209, 110, 208, 205, 109, 207, 101, 203, 204, 102 },
    new List<int> { 101, 104, 105, 108, 110, 203, 106, 303, 305, 306 },

    new List<int> { 102, 101, 109, 103, 203, 204, 205, 104, 105, 302 },
    new List<int> { 109, 203, 103, 205, 209, 101, 102, 105, 104, 110 },
    new List<int> { 205, 108, 203, 101, 103, 105, 104, 210, 209, 109 },
    new List<int> { 210, 207, 203, 204, 103, 302, 108, 304, 105, 110 },
    new List<int> { 307, 101, 103, 104, 208, 207, 204, 304, 105, 102 },
    new List<int> { 109, 101, 102, 103, 104, 105, 106, 108, 110, 203 },
    
    new List<int> { 209, 205, 104, 105, 108, 101, 110, 210, 302, 204 },
    new List<int> { 205, 103, 302, 101, 207, 307, 208, 303, 306, 301 },
    new List<int> { 203, 305, 307, 210, 106, 103, 101, 110, 207, 105 },

    new List<int> { 103, 101, 301, 208, 108, 210, 307, 106, 110, 203 },
    new List<int> { 103, 101, 301, 208, 108, 210, 307, 106, 110, 203 }, 
    new List<int> { 103, 101, 301, 208, 108, 210, 307, 106, 110, 203 },
    new List<int> { 101, 108, 203, 207, 306, 304, 301, 307, 303, 103 },
    new List<int> { 101, 301, 102, 303, 302, 106, 108, 109, 207, 110 },
    new List<int> { 103, 101, 301, 208, 108, 210, 307, 106, 110, 203 },
    new List<int> { 103, 101, 301, 208, 108, 210, 307, 106, 110, 203 }, 
    new List<int> { 303, 210, 307, 306, 304, 207, 101, 102, 103, 104 },
    
    new List<int> { 104, 103, 106, 110, 102, 105, 101, 108, 210, 205 },

    new List<int> { 304, 104, 103, 106, 306, 204, 205, 203, 209, 208 },
    new List<int> { 304, 104, 103, 106, 306, 204, 205, 203, 209, 208 }, 
    new List<int> { 302, 207, 307, 203, 101, 104, 105, 210, 305, 304 },
    new List<int> { 101, 103, 108, 305, 205, 203, 207, 303, 106, 307 },
    new List<int> { 101, 103, 105, 104, 110, 109, 102, 205, 303, 306 },
    new List<int> { 108, 210, 106, 204, 103, 306, 110, 208, 109, 207 },
    new List<int> { 301, 108, 101, 105, 106, 210, 110, 203, 305, 306 },
    new List<int> { 305, 303, 103, 304, 208, 207, 209, 101, 108, 110 },
    new List<int> { 108, 110, 210, 207, 101, 106, 103, 305, 307, 301 },
    new List<int> { 102, 104, 105, 106, 108, 203, 207, 210, 307, 103 },
    new List<int> { 106, 105, 108, 110, 101, 102, 104, 209, 210, 301 },
    new List<int> { 210, 108, 102, 101, 103, 104, 105, 106, 109, 110 },
    new List<int> { 304, 104, 103, 106, 306, 204, 205, 203, 209, 208 },
    new List<int> { 210, 106, 108, 305, 306, 110, 307, 301, 101, 302 },
    new List<int> { 210, 101, 104, 105, 106, 307, 205, 305, 306, 204 }

};



//Creates students for real data test

List<StudentModel> students = new List<StudentModel>();


int userIds = 1; //don't know what these ids are for but they are needed

for (int i = 0; i < names.Count(); i++)
{

    UserModel currentUser = new UserModel
    {
        Id = userIds++,
        IsAdmin = false, //is admin will be false for all students
        Name = names[i],
        Email = $"{names[i]}@uts.edu.au", //emails don't really matter for testing
		IsVerified = true //don't know what this is just made it true for everyone
    };

    StudentModel currentStudent = new StudentModel 
    {
        Id = studentIDs[i],
        WillSignContract = true, // all students in real life example were willing to sign contract
        User = currentUser
    };

    students.Add(currentStudent);
}


//Creates preferences for real data test

List<PreferenceModel> preferences = new List<PreferenceModel>();

int preferenceIDs = 1;

for (int i = 0; i < students.Count(); i++)
{

    float initialStrength = 1.0f;

    foreach (int projectId in individualPreferences[i])
    {
		var currentProject = projects.First(p => p.Id == projectId);

        PreferenceModel currentPreference = new PreferenceModel
        {
            Id = preferenceIDs++,
            Strength = initialStrength,
            Student = students[i],
            Project = currentProject
        };

        preferences.Add(currentPreference);

        initialStrength -= 0.1f; // the preferences are ordered from best to worst so just decrement strength each time we go to the next one
 
    }

}



//this prints student info and preferences
//foreach (PreferenceModel preference in preferences)
//{
//    Console.WriteLine($"prefId: {preference.Id}|{preference.Student.Id}|{preference.Student.User.Name}|{preference.Project.Name}|{preference.Project.Id}|{preference.Strength}");
//}


//Solve

var solver = new AllocationSolver();
var run = new SolveRunModel
{
	PreferenceExponent = 1,
	Timestamp = DateTime.UtcNow,
};
var manualAllocations = new List<AllocationDto>();
var clientLimits = new List<ClientLimitsDto>();
var assignments = solver.AssignStudentsToGroups(run, students, projects, clients, preferences, manualAllocations, clientLimits, 0.5).ToList();


Dictionary<string, List<int>> groupsAndStudents = new Dictionary<string, List<int>>();


if (!assignments.Any())
{
	Console.WriteLine("No assignments");
}

else
{

	foreach (ProjectModel project in projects)
	{
		List<int> testing = new List<int> { };
		groupsAndStudents.Add(project.Name, testing);
	}

	foreach (var assignment in assignments)
	{
		groupsAndStudents[assignment.Project.Name].Add(assignment.Student.Id);
	}


	foreach (ProjectModel project in projects)
	{
		String groupFormatted = $"{project.Name}";
		
		if (groupsAndStudents[project.Name].Count == 0)
		{
			continue;
		}

		
		foreach (var studentID in groupsAndStudents[project.Name])
		{
			groupFormatted += $", {studentID}";
		}

		Console.WriteLine(groupFormatted);
	}

	//foreach (var assignment in assignments)
	//{

	//	int projectRank = 1;

	//	//this only works if the solver returns the students in order which i think it always does
	//	foreach (int preference in individualPreferences[studentIndex])
	//	{
	//		if (assignment.Project.Id == preference)
	//		{
	//			break;
	//		}

	//		projectRank += 1;

	//	}

	//	Console.WriteLine($"{studentCount++}: student:{assignment.Student.User.Name}_{assignment.Student.Id} assigned to project:{assignment.Project.Name}_{assignment.Project.Id} project rank: {projectRank}");

	//	rankTracker[projectRank] += 1;

	//	studentIndex++;

	//}

}

////prints out amount of ranks
//int rankCount = 0;
//foreach (int rankamount in rankTracker)
//{
//	Console.WriteLine($"rank:{rankCount} amount:{rankamount}");
//	rankCount++;
//}
