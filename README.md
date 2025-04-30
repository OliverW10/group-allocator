# GroupAllocator

## Fullstack Setup (Testing)

1. Install Docker Desktop ([https://www.docker.com/products/docker-desktop/](https://www.docker.com/products/docker-desktop/))
2. Run `docker compose up --build --force-recreate` (Ctrl+C to stop)

## Frontend Setup (Development)

1. Install Node.js ([https://volta.sh/](https://volta.sh/))
2. Install PNPM (`npm i -g pnpm`)
3. Install dependencies (`pnpm i`)
4. Run `pnpm dev`

## Backend Setup (Development)

### Database

1. Install docker & docker-compose
1. Run `docker-compose -f database.yml up -d`

```mermaid
erDiagram
    ClientModel {
        int Id
        string Name
        int MinProjects
        int MaxProjects
    }

    FileModel {
        int Id
        byte[] Blob
        string Name
    }

    PreferenceModel {
        int Id
        double Strength
    }
    
    ProjectModel {
        int Id
        string Name
        bool RequiresNda
        int MinStudents
        int MaxStudents
    }

    SolveRunModel {
        int Id
        DateTime Timestamp
        double Evaluation
    }

    StudentAssignmentModel {
        int Id
    }

    StudentModel {
        int Id
        bool WillSignContract
    }

    UserModel {
        int Id
        bool IsAdmin
        string Name
        string Email
        bool IsVerified
    }

    ProjectModel ||--|| ClientModel : belongs_to
    PreferenceModel ||--|| StudentModel : references
    PreferenceModel ||--|| ProjectModel : references
    StudentAssignmentModel ||--|| StudentModel : references
    StudentAssignmentModel ||--|| ProjectModel : references
    StudentAssignmentModel ||--|| SolveRunModel : references
    StudentModel ||--|| UserModel : has_one
    FileModel ||--|| UserModel : belongs_to
    SolveRunModel ||--o{ StudentAssignmentModel : has_many
    StudentModel ||--o{ PreferenceModel : has_many
    UserModel ||--o{ FileModel : has_many
```

### Application - Windows

1. Install Visual Studio with ASP.NET workload and .NET 9.0 component
2. In the Group-allocator/backend/group-allocator directory, run the following commands:
   1. `dotnet tool restore` 
   2. `dotnet run`

### Application - Mac/Linux

1. Install [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
1. In `backend/GroupAllocator.Backend` run `dotnet run`

![Cool GIF](readme-images/help.gif)
