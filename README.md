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
    Client {
        int Id PK
        string Name
        int MinProjects
        int MaxProjects
    }

    File {
        int Id PK
        int UserId FK
        byte[] Blob
        string Name
    }

    Preference {
        int Id PK
        int StudentId FK
        int ProjectId FK
        double Strength
    }
    
    Project {
        int Id PK
        int ClientId FK
        string Name
        bool RequiresNda
        int MinStudents
        int MaxStudents
    }

    SolveRun {
        int Id PK
        DateTime Timestamp
        double Evaluation
    }

    StudentAssignment {
        int Id PK
        int StudentId FK
        int ProjectId FK
        int SolveRunId FK
    }

    Student {
        int Id PK
        int UserId FK
        bool WillSignContract
    }

    User {
        int Id PK
        bool IsAdmin
        string Name
        string Email
        bool IsVerified
    }

    Project }o--|| Client : has_many
    Preference }o--|| Student : has_many
    Preference }o--|| Project : has_many
    StudentAssignment }o--|| Student : has_many
    StudentAssignment }o--|| Project : has_many
    StudentAssignment }|--|| SolveRun : references
    Student |o--|| User : has_one
    User ||--o{ File : has_many
```

### Application - Windows

1. Install Visual Studio with ASP.NET workload and .NET 9.0 component
2. In the Group-allocator/backend/group-allocator directory, run the following commands:
   1. `dotnet tool restore` 
   2. `dotnet run`

### Application - Mac/Linux

1. Install [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
1. In `backend/GroupAllocator.Backend` run `dotnet run`
****